using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.SignalRDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;

    public UserController(
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository)
    {
        _mapper = mapper;
        _userManager = userManager;
        _chatHubContext = chatHubContext;
        _userRepository = userRepository;
    }

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

        var profileProjection = _mapper.Map<UserProfileProjection>(_userRepository.GetUserProfile(UserId));
        profileProjection.UserId = UserId;
        profileProjection.UserName = UserName;
        profileProjection.Email = UserEmail;

        return Ok(profileProjection);
    }

    [HttpPost("connection-request")]
    public async Task<IActionResult> ConnectionRequest([FromBody] ConnectionRequestDto invitationDto)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest();

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest();

        var connectionExists = _userRepository.ConnectionExists(UserId, invitedUser.Id);
        if (connectionExists) return BadRequest();

        var newConnection = _userRepository.CreateConnection(UserId, UserName, invitedUser.Id, invitedUser.UserName);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(invitedUser.Id).ConnectionRequestReceived(_mapper.Map<UserConnectionDto>(newConnection));
            await _chatHubContext.Clients.Group(UserId).ConnectionRequestReceived(_mapper.Map<UserConnectionDto>(newConnection));
            return Ok();
        }

        throw new Exception("Error when creating connection request");
    }

    [HttpDelete("connection-delete")]
    public async Task<IActionResult> DeleteConnection(long connectionId)
    {
        var connection = await _userRepository.GetConnection(connectionId);
        if (connection is null) return BadRequest();

        if (connection.Blocked && !connection.BlockedById.Equals(UserId))
        {
            return BadRequest();
        }

        var connectionDto = _mapper.Map<UserConnectionDto>(connection);
        if (!connectionDto.UserPresent(UserId))
        {
            return BadRequest();
        }

        var invited = connection.InvitedId;
        var inviter = connection.InviterId;

        _userRepository.DeleteConnection(connectionId);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(invited).ConnectionDeleted(new ResourceId(connectionId));
            await _chatHubContext.Clients.Group(inviter).ConnectionDeleted(new ResourceId(connectionId));
            return Ok();
        }

        return BadRequest();
    }

    [HttpPost("connection-block")]
    public async Task<IActionResult> BlockConnection(long connectionId)
    {
        var connection = await _userRepository.GetConnection(connectionId);
        if (connection is null) return BadRequest();

        return BadRequest();
    }

    [HttpPut("update-email")]
    public async Task<IActionResult> UpdateEmail(
        IEmailClient emailClient,
        IOptionsMonitor<CorsSettings> corsSettings,
        [FromBody] EmailForm emailForm)
    {
        if (emailForm.Equals(UserEmail))
        {
            return BadRequest();
        }

        var currentUser = await _userManager.FindByIdAsync(UserId);
        var confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(currentUser, emailForm.Email);

        var qb = new QueryBuilder
        {
            { "token", confirmationToken },
            { "email", emailForm.Email }
        };
        var callbackUrl = $@"{corsSettings.CurrentValue.ApiUrl}/account/confirm-email/{qb}";

        var sgResponse = await emailClient.SendEmailConfirmationAsync(emailForm.Email, callbackUrl);
        if (sgResponse.StatusCode != HttpStatusCode.Accepted)
        {
            return Problem();
        }
        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(string token, string email)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        var currentUser = await _userManager.FindByIdAsync(UserId);
        var confirmResult = await _userManager.ChangeEmailAsync(currentUser, email, token);

        if (!confirmResult.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }
}
