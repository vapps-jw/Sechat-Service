using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Service.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sechat.Service.Controllers.AdminControllerForms;

namespace Sechat.Service.Controllers;

[Authorize(AppConstants.AuthorizationPolicy.AdminPolicy)]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class AdminController : SechatControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;

    public AdminController(
        ILogger<AdminController> logger,
        IDbContextFactory<SechatContext> contextFactory)
    {
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
        var result = await ctx.SaveChangesAsync();
        return result > 0 ? Ok() : BadRequest("Setting not updated");
    }
}

public class AdminControllerForms
{
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

