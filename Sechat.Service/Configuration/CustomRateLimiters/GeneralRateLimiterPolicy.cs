using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.CustomRateLimiters;

public class GeneralRateLimiterPolicy : IRateLimiterPolicy<string>
{
    public RateLimitPartition<string> GetPartition(HttpContext httpContext) =>
        httpContext.User.Identity?.IsAuthenticated == true
            ? RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity.Name!,
                partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                })
            : RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers.Host.ToString(),
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
            });

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } =
    (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 418;
        return new ValueTask();
    };
}
