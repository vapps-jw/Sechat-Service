using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration.CustomRateLimiters;
using Sechat.Service.Utilities;
using System;
using System.Threading.RateLimiting;

namespace Sechat.Service.Configuration.Installers;

public class RateLimiterInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => _ = webApplicationBuilder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                SegmentsPerWindow = 6,
                PermitLimit = 100,
                QueueLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));

        options.RejectionStatusCode = 429;
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimiterInstaller>>();
            logger.LogWarning("Server Overloaded by {user}",
                context.HttpContext.User.Identity?.Name ?? context.HttpContext.Connection.RemoteIpAddress.ToString() ?? context.HttpContext.Request.Headers.Host.ToString());
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                await context.HttpContext.Response.WriteAsync($"Server Overloaded. Try again after {retryAfter.TotalMinutes} minute(s)", cancellationToken: token);
            }
            else
            {
                await context.HttpContext.Response.WriteAsync("Server Overloaded", cancellationToken: token);
            }
        };

        _ = options.AddPolicy(AppConstants.RateLimiting.MinimalRateLimiterPolicy, new MinimalRateLimiterPolicy());

    });
}
