using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
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
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                await context.HttpContext.Response.WriteAsync("Server Overloaded");
            }
            else
            {
                await context.HttpContext.Response.WriteAsync("Server Overloaded");
            }
        };

        //_ = options.AddPolicy(AppConstants.RateLimiting.GeneralCustomPolicy, new GeneralRateLimiterPolicy());

        _ = options.AddPolicy(AppConstants.RateLimiting.DefaultWindowPolicyName, httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                SegmentsPerWindow = 6,
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1)
            }));

    });
}
