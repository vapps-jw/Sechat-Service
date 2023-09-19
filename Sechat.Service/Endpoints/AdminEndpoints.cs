using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Service.Configuration;
using Sechat.Service.Controllers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Endpoints;

public static class AdminEndpoints
{
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("v2/admin").RequireAuthorization(AppConstants.AuthorizationPolicy.AdminPolicy);
        _ = group.MapGet("global-settings", GetGlobalSettings).WithName(nameof(GetGlobalSettings));
        _ = group.MapPatch("global-setting", UpdateGlobalSettings).WithName(nameof(UpdateGlobalSettings));
    }

    public static async Task<IResult> GetGlobalSettings(
        ILogger<AdminController> _logger,
        HttpContext http,
        CancellationToken cancellationToken,
        IDbContextFactory<SechatContext> contextFactory)
    {
        _logger.LogWarning("Global settings requested");
        using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await ctx.GlobalSettings.ToListAsync(cancellationToken);

        http.Response.Headers.CacheControl = Utilities.ResponseHeaders.NoStore;
        return Results.Ok(result);
    }

    public static async Task<IResult> UpdateGlobalSettings(
        [FromBody] SettingForm form,
        ILogger<AdminController> _logger,
        CancellationToken cancellationToken,
        IDbContextFactory<SechatContext> contextFactory)
    {
        _logger.LogWarning("Global Setting change requested {req}", form);
        using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var setting = ctx.GlobalSettings.FirstOrDefault(s => s.Id.Equals(form.Id));
        setting.Value = form.Value;

        var result = await ctx.SaveChangesAsync();
        return result > 0 ? Results.Ok() : Results.BadRequest("Setting not updated");
    }

    public class SettingForm
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
