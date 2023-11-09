using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Service.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sechat.Service.Controllers.AdminControllerForms;

namespace Sechat.Service.Controllers;

[Route("[controller]")]
[Authorize(AppConstants.AuthorizationPolicy.AdminPolicy)]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class AdminController : SechatControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AdminController> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;

    public AdminController(
        UserManager<IdentityUser> userManager,
        ILogger<AdminController> logger,
        IDbContextFactory<SechatContext> contextFactory)
    {
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
        return Ok(result);
    }

    [HttpPatch("global-setting")]
    public async Task<IActionResult> UpdateGlobalSettings([FromBody] SettingForm form, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Global Setting change requested {req}", form);
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var setting = ctx.GlobalSettings.FirstOrDefault(s => s.Id.Equals(form.Id));
        setting.Value = form.Value;
        var result = await ctx.SaveChangesAsync(cancellationToken);
        return result > 0 ? Ok() : BadRequest("Setting not updated");
    }

    [HttpPost("lock-user")]
    public async Task<IActionResult> LockUserByUserName([FromBody] UserIdentifier userIdentifier)
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
    public async Task<IActionResult> UnlockUserByUserName([FromBody] UserIdentifier userIdentifier)
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
            .Select(u => new RichUserIdentifier()
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

