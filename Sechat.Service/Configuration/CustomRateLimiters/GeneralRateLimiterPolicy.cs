using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration.Installers;
using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.CustomRateLimiters;

public class MinimalRateLimiterPolicy : IRateLimiterPolicy<string>
{
    public RateLimitPartition<string> GetPartition(HttpContext httpContext) =>
        httpContext.User.Identity?.IsAuthenticated == true
            ? RateLimitPartition.GetSlidingWindowLimiter(httpContext.User.Identity.Name!,
                partition => new SlidingWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    SegmentsPerWindow = 6,
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                })
            : RateLimitPartition.GetSlidingWindowLimiter(httpContext.Request.Headers.Host.ToString(),
            partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                SegmentsPerWindow = 6,
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
            });

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected { get; } = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimiterInstaller>>();
        logger.LogWarning("Server Overloaded by {user}", context.HttpContext.User.Identity?.Name ?? context.HttpContext.Request.Headers.Host.ToString());
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {

            await context.HttpContext.Response.WriteAsync($"Server Overloaded - Minimal Rate. Try again after {retryAfter.TotalMinutes} minute(s)");
        }
        else
        {

            await context.HttpContext.Response.WriteAsync("Server Overloaded - Minimal Rate");
        }
    };
}
