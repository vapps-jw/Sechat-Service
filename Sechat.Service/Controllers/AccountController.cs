﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.SignalRDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController : SechatControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserRepository _userRepository;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public AccountController(
        IMapper mapper,
        ILogger<AccountController> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        UserRepository userRepository,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _chatHubContext = chatHubContext;
    }

    [HttpGet("test-secret")]
    public IActionResult AuthTest() => Ok("SECRET");

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserCredentials userCredentials)
    {
        var signInResult = await _signInManager.PasswordSignInAsync(userCredentials.Username, userCredentials.Password, true, false);
        return !signInResult.Succeeded ? BadRequest() : Ok();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCredentials userCredentials)
    {
        var user = new IdentityUser(userCredentials.Username);
        var createUserResult = await _userManager.CreateAsync(user, userCredentials.Password);

        if (!createUserResult.Succeeded)
        {
            return BadRequest();
        }

        _logger.LogInformation($"User {userCredentials.Username} has been created");
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager
            .FindByIdAsync(User.Claims
            .FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value);
        if (user is null)
        {
            return BadRequest("User not found");
        }

        var deleteResult = await _userRepository.DeleteUserProfile(UserId);

        if (await _userRepository.SaveChanges() > 0)
        {
            foreach (var ownedRoom in deleteResult.OwnedRooms)
            {
                await _chatHubContext.Clients.Group(ownedRoom).RoomDeleted(new ResourceGuid(ownedRoom));
            }

            foreach (var memberRoom in deleteResult.MemberRooms)
            {
                await _chatHubContext.Clients.Group(memberRoom).UserRemovedFromRoom(new UserRemovedFromRoom(memberRoom, UserName));
            }

            foreach (var connection in deleteResult.Connections)
            {
                await _chatHubContext.Clients.Group(connection.InvitedId).ConnectionDeleted(new ResourceId(connection.Id));
                await _chatHubContext.Clients.Group(connection.InviterId).ConnectionDeleted(new ResourceId(connection.Id));
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Problem();
        }

        _logger.LogInformation($"User {UserName} has been removed");
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordForm passwordForm)
    {
        var currentUser = await _userManager.FindByIdAsync(UserId);
        if (currentUser is null)
        {
            return BadRequest();
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(currentUser, passwordForm.OldPassword, passwordForm.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return Problem();
        }

        await _signInManager.RefreshSignInAsync(currentUser);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] EmailForm emailForm,
        IEmailClient emailClient,
        IOptionsMonitor<CorsSettings> corsSettings)
    {
        var currentUser = await _userManager.FindByEmailAsync(emailForm.Email);
        if (currentUser is null)
        {
            return Ok();
        }

        var confirmationToken = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
        var qb = new QueryBuilder
        {
            { "token", confirmationToken },
            { "userId", UserId },
        };
        var callbackUrl = $@"{corsSettings.CurrentValue.WebAppUrl}/account/reset-password/{qb}";

        var sgResponse = await emailClient.SendPasswordResetAsync(emailForm.Email, callbackUrl);
        return sgResponse.StatusCode != HttpStatusCode.Accepted ? Problem() : Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetForm passwordResetForm)
    {
        var currentUser = await _userManager.FindByIdAsync(passwordResetForm.UserId);
        var confirmResult = await _userManager.ResetPasswordAsync(currentUser, passwordResetForm.Token, passwordResetForm.NewPassword);

        return !confirmResult.Succeeded ? BadRequest() : Ok();
    }
}
