using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Route("[controller]")]
[Authorize(AppConstants.AuthorizationPolicy.AdminPolicy)]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class AdminController : SechatControllerBase
{
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AdminController> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;

    public AdminController(
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository,
        UserManager<IdentityUser> userManager,
        ILogger<AdminController> logger,
        IDbContextFactory<SechatContext> contextFactory)
    {
        _chatHubContext = chatHubContext;
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    [HttpGet("global-settings")]
    public async Task<IActionResult> GetGlobalSettings(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Global Settings requested");
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await ctx.GlobalSettings.ToListAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return Ok(result);
    }

    [HttpPatch("global-setting")]
    public async Task<IActionResult> UpdateGlobalSettings([FromBody] AdminControllerForms.SettingForm form, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Global Setting change requested {req}", form);
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var setting = ctx.GlobalSettings.FirstOrDefault(s => s.Id.Equals(form.Id));
        setting.Value = form.Value;
        var result = await ctx.SaveChangesAsync(cancellationToken);
        return result > 0 ? Ok() : BadRequest("Setting not updated");
    }

    [HttpGet("usernames")]
    public IActionResult GetUserNames(CancellationToken cancellationToken) => Ok(_userManager.Users.Select(iu => iu.UserName).OrderBy(u => u).ToList());

    [HttpDelete("delete-account/{userName}")]
    public async Task<IActionResult> DeleteAccount(string userName, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return BadRequest("User not found");
        }

        var deleteResult = await _userRepository.DeleteUserProfile(user.Id);

        if (await _userRepository.SaveChanges(cancellationToken) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
        return Ok();
    }

    [HttpPost("lock-user")]
    public async Task<IActionResult> LockUserByUserName([FromBody] AdminControllerForms.UserIdentifier userIdentifier)
    {
        var user = await _userManager.FindByNameAsync(userIdentifier.UserName);
        if (user is null) return BadRequest("User does not exit");

        var lockUserTask = await _userManager.SetLockoutEnabledAsync(user, true);
        if (lockUserTask.Succeeded)
        {
            lockUserTask = await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
            return lockUserTask.Succeeded ? Ok() : BadRequest("Lockout failed");
        }
        else
        {
            return BadRequest("Lockout failed");
        }
    }

    [HttpPost("unlock-user")]
    public async Task<IActionResult> UnlockUserByUserName([FromBody] AdminControllerForms.UserIdentifier userIdentifier)
    {
        var user = await _userManager.FindByNameAsync(userIdentifier.UserName);
        if (user is null) return BadRequest("User does not exit");

        var unlockUserTask = await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(-100));
        return unlockUserTask.Succeeded ? Ok() : BadRequest("Lockout failed");
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = _userManager.Users
            .Select(u => new AdminControllerForms.RichUserIdentifier()
            {
                Email = u.Email,
                LockoutEnd = u.LockoutEnd,
                UserName = u.UserName,
            })
            .ToList();
        return Ok(users);
    }

    [HttpPost("extract")]
    public IActionResult Extract(CancellationToken cancellationToken) => Ok();
}

public class AdminControllerForms
{
    public class UserIdentifier
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class UserIdentifierValidation : AbstractValidator<UserIdentifier>
    {
        public UserIdentifierValidation()
        {
            _ = RuleFor(x => x.UserName).NotNull().NotEmpty();
            _ = RuleFor(x => x.Email).EmailAddress();
        }
    }

    public class RichUserIdentifier : UserIdentifier
    {
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    public class SettingForm
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
    public class SettingFormValidation : AbstractValidator<SettingForm>
    {
        public SettingFormValidation()
        {
            _ = RuleFor(x => x.Id).NotNull().NotEmpty().MaximumLength(AppConstants.StringLength.NameMax);
            _ = RuleFor(x => x.Value).NotNull().NotEmpty().MaximumLength(AppConstants.StringLength.NameMax);
        }
    }
}

