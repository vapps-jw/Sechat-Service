﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Data;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class AccountController : SechatControllerBase
{
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly IOptionsMonitor<CorsSettings> _corsSettings;
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserRepository _userRepository;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public AccountController(
        IDbContextFactory<SechatContext> contextFactory,
        IOptionsMonitor<CorsSettings> corsSettings,
        ILogger<AccountController> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        UserRepository userRepository,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _contextFactory = contextFactory;
        _corsSettings = corsSettings;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _chatHubContext = chatHubContext;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> SignIn(
        [FromServices] UserDataService userDataService,
        [FromBody] UserCredentials userCredentials,
        CancellationToken cancellationToken)
    {
        var signInResult = await _signInManager.PasswordSignInAsync(userCredentials.Username, userCredentials.Password, true, true);
        cancellationToken.ThrowIfCancellationRequested();

        if (signInResult.Succeeded)
        {
            _logger.LogWarning("User Logged In: {Username}", userCredentials.Username);
            var user = await _userManager.FindByNameAsync(userCredentials.Username);
            var profile = await userDataService.GetProfile(user.Id, user.UserName);
            return Ok(profile);
        }

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("User Locked: {Username}", userCredentials.Username);
            return BadRequest("Account locked, try again later");
        }

        _logger.LogWarning("User failed to Sign In: {Username} Reason: {reason}", userCredentials.Username, signInResult.ToString());
        return BadRequest("Failed to Sign In");
    }

    [AllowAnonymous]
    [HttpPost("login/token")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> SignInForToken(
        [FromServices] TokenService tokenService,
        [FromBody] UserCredentials userCredentials)
    {
        var user = await _userManager.FindByNameAsync(userCredentials.Username);
        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, false);
        if (!passwordCheck.Succeeded)
        {
            return BadRequest();
        }

        var token = await tokenService.GenerateToken(userCredentials.Username);
        return string.IsNullOrEmpty(token) ? BadRequest() : Ok(token);
    }

    [HttpPost("create/token")]
    public async Task<IActionResult> CreateToken(
        [FromServices] TokenService tokenService)
    {
        var token = await tokenService.GenerateToken(UserName);
        return string.IsNullOrEmpty(token) ? BadRequest() : Ok(token);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> SignUp([FromBody] SignUpDetails signUpDetails, CancellationToken cancellationToken)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var result = ctx.GlobalSettings.FirstOrDefault(s => s.Id.Equals(AppGlobalSettings.SettingName.RegistrationStatus));
        if (result.Value.Equals(AppGlobalSettings.RegistrationStatus.Forbidden))
        {
            return BadRequest("New User registration turned off temporarily");
        }

        var user = new IdentityUser(signUpDetails.Username)
        {
            LockoutEnabled = true
        };

        cancellationToken.ThrowIfCancellationRequested();
        var createUserResult = await _userManager.CreateAsync(user, signUpDetails.Password);

        if (!createUserResult.Succeeded)
        {
            return BadRequest("Failed to Sign Up");
        }

        _logger.LogInformation($"User {signUpDetails.Username} has been created");
        return Ok();
    }

    [HttpPost("ask-for-chat")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> AskForChatAccess(
        UserManager<IdentityUser> userManager,
        [FromBody] ReferralPass referallPass,
        SechatContext context)
    {
        var referralValidation = context.UserProfiles.Any(p => p.ReferralPass.Equals(referallPass.PassPhrase));
        if (!referralValidation)
        {
            return BadRequest("Check your Referall");
        }

        var user = await userManager.GetUserAsync(User);
        var currentClaims = (await userManager.GetClaimsAsync(user))
           .Where(c => c.Type.Equals(AppConstants.ClaimType.ServiceClaim))
           .Select(c => c.Value)
           .ToList();

        if (currentClaims.Any(cc => cc.Equals(AppConstants.ServiceClaimValue.ChatAccess)))
        {
            return BadRequest("You already have access");
        }

        var addResult = await userManager.AddClaimAsync(user, new Claim(AppConstants.ClaimType.ServiceClaim, AppConstants.ServiceClaimValue.ChatAccess));
        return addResult.Succeeded ? Ok() : Problem();
    }

    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims as List<Claim>;
        var reuslt = claims
              .Where(c => c.Type.Equals(AppConstants.ClaimType.ServiceClaim))
              .Select(c => c.Value)
              .ToList();

        return Ok(reuslt);
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
                await _chatHubContext.Clients.Group(memberRoom).UserRemovedFromRoom(new RoomUserActionMessage(memberRoom, UserName));
            }

            foreach (var connection in deleteResult.Connections)
            {
                await _chatHubContext.Clients.Group(connection.InvitedId).ContactDeleted(new ResourceId(connection.Id));
                await _chatHubContext.Clients.Group(connection.InviterId).ContactDeleted(new ResourceId(connection.Id));
            }
        }

        _ = await _userManager.UpdateSecurityStampAsync(user);
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordForm passwordForm)
    {
        var currentUser = await _userManager.FindByIdAsync(UserId);
        if (currentUser is null)
        {
            return BadRequest("User not found");
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(currentUser, passwordForm.OldPassword, passwordForm.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            return BadRequest("Password change failed");
        }

        await _signInManager.RefreshSignInAsync(currentUser);
        return Ok();
    }

    [HttpPost("referral-pass")]
    public async Task<IActionResult> UpdateReferallPass([FromBody] ReferralPass referallPass, CancellationToken cancellationToken)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var profile = ctx.UserProfiles.FirstOrDefault(p => p.Id.Equals(UserId));

        cancellationToken.ThrowIfCancellationRequested();

        if (profile is null)
        {
            return BadRequest("You dont have a Profile?");
        }

        profile.ReferralPass = referallPass.PassPhrase;
        return await ctx.SaveChangesAsync() > 0 ? Ok() : BadRequest("Something went wrong");
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] EmailForm emailForm,
        IEmailClient emailClient)
    {
        var currentUser = await _userManager.FindByEmailAsync(emailForm.Email);
        if (currentUser is null)
        {
            return Ok($"Email sent to {emailForm.Email}");
        }

        var confirmationToken = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
        var qb = new QueryBuilder
        {
            { "token", confirmationToken },
            { "email", emailForm.Email },
        };
        var callbackUrl = $@"{_corsSettings.CurrentValue.WebAppUrl}/user/resetPassword/{qb}";

        var sgResponse = await emailClient.SendPasswordResetAsync(emailForm.Email, callbackUrl);
        return sgResponse.StatusCode != HttpStatusCode.Accepted ? Problem() : Ok($"Email sent to {emailForm.Email}");
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetForm passwordResetForm)
    {
        var currentUser = await _userManager.FindByEmailAsync(passwordResetForm.Email);
        if (currentUser is null)
        {
            return BadRequest(AppConstants.ApiResponseMessage.DefaultFail);
        }
        var confirmResult = await _userManager.ResetPasswordAsync(currentUser, passwordResetForm.Token, passwordResetForm.NewPassword);

        return !confirmResult.Succeeded ? BadRequest(AppConstants.ApiResponseMessage.DefaultFail) : Ok("Password has been changed");
    }

    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail(
    IEmailClient emailClient,
    [FromBody] EmailForm emailForm)
    {
        var user = await _userManager.FindByNameAsync(UserName);
        if (emailForm.Email.Equals(user.Email))
        {
            return BadRequest("Email is the same");
        }

        var currentUser = await _userManager.FindByIdAsync(UserId);
        var confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(currentUser, emailForm.Email);

        var qb = new QueryBuilder
        {
            { "token", confirmationToken },
            { "userName", UserName },
            { "email", emailForm.Email }
        };
        var callbackUrl = $@"{_corsSettings.CurrentValue.WebAppUrl}/user/confirmEmail/{qb}";

        var sgResponse = await emailClient.SendEmailConfirmationAsync(emailForm.Email, callbackUrl);
        return sgResponse.StatusCode != HttpStatusCode.Accepted ? Problem() : Ok($"Email sent to {emailForm.Email}");
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailForm confirmEmailForm)
    {
        var currentUser = await _userManager.FindByNameAsync(confirmEmailForm.UserName);
        var confirmResult = await _userManager.ChangeEmailAsync(currentUser, confirmEmailForm.Email, confirmEmailForm.Token);

        return !confirmResult.Succeeded ? BadRequest("Email has not been confirmed") : Ok($"Email {confirmEmailForm.Email} confirmed");
    }
}
