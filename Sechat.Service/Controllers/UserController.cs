using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly CryptographyService _cryptographyService;
    private readonly PushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;

    public UserController(
          CryptographyService cryptographyService,
        PushNotificationService pushNotificationService,
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository)
    {
        _cryptographyService = cryptographyService;
        _pushNotificationService = pushNotificationService;
        _mapper = mapper;
        _userManager = userManager;
        _chatHubContext = chatHubContext;
        _userRepository = userRepository;
    }

    private async Task<string> GetUserId(string userName) => (await _userManager.FindByNameAsync(userName))?.Id;

    [HttpGet("get-profile")]
    public async Task<IActionResult> GetProfile()
    {
        _userRepository.UpdateUserActivity(UserId);

        if (!_userRepository.ProfileExists(UserId))
        {
            _userRepository.CreateUserProfile(UserId, UserName);
            if (await _userRepository.SaveChanges() == 0)
            {
                return Problem("Profile creation failed");
            }
        }
        if (!_userRepository.KeyExists(UserId, Data.KeyType.DefaultEncryption))
        {
            var newKey = _cryptographyService.GenerateKey();
            _userRepository.UpdatKey(UserId, Data.KeyType.DefaultEncryption, newKey);
            if (await _userRepository.SaveChanges() == 0)
            {
                return Problem("Defauly Key creating failed");
            }
        }

        var user = await _userManager.FindByNameAsync(UserName);
        var profileProjection = _mapper.Map<UserProfileProjection>(_userRepository.GetUserProfile(UserId));
        profileProjection.UserId = UserId;
        profileProjection.UserName = UserName;
        profileProjection.Email = user.Email;
        profileProjection.EmailConfirmed = user.EmailConfirmed;

        return Ok(profileProjection);
    }

    [HttpPost("request-contact")]
    public async Task<IActionResult> ContactRequest([FromBody] ConnectionRequestDto invitationDto)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest("You cant invite yourself :)");

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest("No one was invited");

        var contactExists = _userRepository.ContactExists(UserId, invitedUser.Id);
        if (contactExists) return BadRequest("Contact exists");

        var newKey = _cryptographyService.GenerateKey();
        var newContact = _userRepository.CreateContact(UserId, UserName, invitedUser.Id, invitedUser.UserName, newKey);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(invitedUser.Id).ConnectionRequestReceived(_mapper.Map<ContactDto>(newContact));
            await _chatHubContext.Clients.Group(UserId).ConnectionRequestReceived(_mapper.Map<ContactDto>(newContact));
            await _pushNotificationService.IncomingContactRequestNotification(invitedUser.Id, UserName);
            return Ok("Invitation sent");
        }

        throw new Exception("Error when creating connection request");
    }

    [HttpDelete("delete-contact")]
    public async Task<IActionResult> DeleteContact(long contactId)
    {
        var contact = await _userRepository.GetContact(contactId);
        if (contact is null) return BadRequest("Not your contact");

        if (contact.Blocked && !contact.BlockedById.Equals(UserId))
        {
            return BadRequest("You are blocked");
        }

        var contactDto = _mapper.Map<ContactDto>(contact);
        if (!contactDto.UserPresent(UserName))
        {
            return BadRequest("Not your contact");
        }

        _userRepository.DeleteContact(contactId);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(contact.InvitedId).ConnectionDeleted(new ResourceId(contactId));
            await _chatHubContext.Clients.Group(contact.InviterId).ConnectionDeleted(new ResourceId(contactId));
            return Ok();
        }

        return BadRequest("Something went wrong");
    }

    [HttpPatch("block-contact")]
    public async Task<IActionResult> BlockContact(long contactId)
    {
        var contact = _userRepository.BlockContact(contactId, UserId, UserName);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InvitedName)).ConnectionUpdated(contactDto);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InviterName)).ConnectionUpdated(contactDto);
            return Ok();
        }

        return BadRequest("Cant do that");
    }

    [HttpPatch("allow-contact")]
    public async Task<IActionResult> AllowContact(long contactId)
    {
        var contact = _userRepository.AllowContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            await _chatHubContext.Clients.Group(contact.InvitedId).ConnectionUpdated(contactDto);
            await _chatHubContext.Clients.Group(contact.InviterId).ConnectionUpdated(contactDto);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("contact-e2e")]
    public async Task<IActionResult> ChangeEncryption(long contactId, bool e2e)
    {
        if (_userRepository.CheckContactWithMessages(contactId, UserId, out var contact))
        {
            contact.EncryptedByUser = e2e;
            contact.DirectMessages.Clear();
            _ = await _userRepository.SaveChanges();

            await _chatHubContext.Clients.Group(contact.InviterId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
            await _chatHubContext.Clients.Group(contact.InvitedId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
            return Ok(_mapper.Map<ContactDto>(contact));

        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("approve-contact")]
    public async Task<IActionResult> ApproveContact(
        [FromServices] Channel<DefaultNotificationDto> channel,
        long contactId)
    {
        var contact = _userRepository.ApproveContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            var inviterId = await GetUserId(contactDto.InviterName);

            contactDto.ContactState = AppConstants.ContactState.Online;

            await _chatHubContext.Clients.Group(UserId).ConnectionUpdated(contactDto);
            await _chatHubContext.Clients.Group(inviterId).ConnectionUpdated(contactDto);
            await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.ContactRequestApproved, inviterId, UserName));
            return Ok();
        }

        return BadRequest("Can`t do that");
    }
}
