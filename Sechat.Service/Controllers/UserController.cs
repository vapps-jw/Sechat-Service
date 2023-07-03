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
        if (!_userRepository.ProfileExists(UserId))
        {
            _userRepository.CreateUserProfile(UserId, UserName);
            if (await _userRepository.SaveChanges() == 0)
            {
                return Problem("Profile creation failed");
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

    [HttpPost("request-connection")]
    public async Task<IActionResult> ContactRequest([FromBody] ConnectionRequestDto invitationDto)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest("You cant invite yourself :)");

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest("No one was invited");

        var contactExists = _userRepository.ContactExists(UserId, invitedUser.Id);
        if (contactExists) return BadRequest("Contact exists");

        var newKey = _cryptographyService.GenerateStringKey();
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

    [HttpDelete("delete-connection")]
    public async Task<IActionResult> DeleteContact(long connectionId)
    {
        var connection = await _userRepository.GetContact(connectionId);
        if (connection is null) return BadRequest("Not your contact");

        if (connection.Blocked && !connection.BlockedById.Equals(UserId))
        {
            return BadRequest("You are blocked");
        }

        var connectionDto = _mapper.Map<ContactDto>(connection);
        if (!connectionDto.UserPresent(UserName))
        {
            return BadRequest("Not your contact");
        }

        _userRepository.DeleteContact(connectionId);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(connection.InvitedId).ConnectionDeleted(new ResourceId(connectionId));
            await _chatHubContext.Clients.Group(connection.InviterId).ConnectionDeleted(new ResourceId(connectionId));
            return Ok();
        }

        return BadRequest("Something went wrong");
    }

    [HttpPatch("block-connection")]
    public async Task<IActionResult> BlockContact(long connectionId)
    {
        var connection = _userRepository.BlockContact(connectionId, UserId, UserName);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var connectionDto = _mapper.Map<ContactDto>(connection);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InvitedName)).ConnectionUpdated(connectionDto);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InviterName)).ConnectionUpdated(connectionDto);
            return Ok();
        }

        return BadRequest("Cant do that");
    }

    [HttpPatch("allow-connection")]
    public async Task<IActionResult> AllowContact(long connectionId)
    {
        var connection = _userRepository.AllowContact(connectionId, UserId);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var connectionDto = _mapper.Map<ContactDto>(connection);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InvitedName)).ConnectionUpdated(connectionDto);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InviterName)).ConnectionUpdated(connectionDto);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("approve-connection")]
    public async Task<IActionResult> ApproveContact(
        [FromServices] Channel<DefaultNotificationDto> channel,
        long connectionId)
    {
        var connection = _userRepository.ApproveContact(connectionId, UserId);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(connection);
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
