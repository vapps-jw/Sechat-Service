using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly PushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;

    public UserController(
        PushNotificationService pushNotificationService,
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository)
    {
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
        profileProjection.Email = UserEmail;
        profileProjection.EmailConfirmed = user.EmailConfirmed;

        return Ok(profileProjection);
    }

    [HttpPost("request-connection")]
    public async Task<IActionResult> ConnectionRequest([FromBody] ConnectionRequestDto invitationDto)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest("You cant invite yourself :)");

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest("No one was invited");

        var connectionExists = _userRepository.ContactExists(UserId, invitedUser.Id);
        if (connectionExists) return BadRequest("Contact exists");

        var newConnection = _userRepository.CreateContact(UserId, UserName, invitedUser.Id, invitedUser.UserName);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(invitedUser.Id).ConnectionRequestReceived(_mapper.Map<UserContactDto>(newConnection));
            await _chatHubContext.Clients.Group(UserId).ConnectionRequestReceived(_mapper.Map<UserContactDto>(newConnection));
            await _pushNotificationService.IncomingContactRequestNotification(invitedUser.Id, UserName);
            return Ok();
        }

        throw new Exception("Error when creating connection request");
    }

    [HttpDelete("delete-connection")]
    public async Task<IActionResult> DeleteConnection(long connectionId)
    {
        var connection = await _userRepository.GetContact(connectionId);
        if (connection is null) return BadRequest("Not your contact");

        if (connection.Blocked && !connection.BlockedById.Equals(UserId))
        {
            return BadRequest("You are blocked");
        }

        var connectionDto = _mapper.Map<UserContactDto>(connection);
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
    public async Task<IActionResult> BlockConnection(long connectionId)
    {
        var connection = _userRepository.BlockContact(connectionId, UserId, UserName);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var connectionDto = _mapper.Map<UserContactDto>(connection);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InvitedName)).ConnectionUpdated(connectionDto);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InviterName)).ConnectionUpdated(connectionDto);
            return Ok();
        }

        return BadRequest("Cant do that");
    }

    [HttpPatch("allow-connection")]
    public async Task<IActionResult> AllowConnection(long connectionId)
    {
        var connection = _userRepository.AllowContact(connectionId, UserId);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var connectionDto = _mapper.Map<UserContactDto>(connection);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InvitedName)).ConnectionUpdated(connectionDto);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InviterName)).ConnectionUpdated(connectionDto);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("approve-connection")]
    public async Task<IActionResult> ApproveConnection(long connectionId)
    {
        var connection = _userRepository.ApproveContact(connectionId, UserId);
        if (connection is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var connectionDto = _mapper.Map<UserContactDto>(connection);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InvitedName)).ConnectionUpdated(connectionDto);
            await _chatHubContext.Clients.Group(await GetUserId(connectionDto.InviterName)).ConnectionUpdated(connectionDto);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }
}
