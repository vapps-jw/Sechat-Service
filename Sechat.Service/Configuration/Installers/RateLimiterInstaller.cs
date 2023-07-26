using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.RateLimiting;

namespace Sechat.Service.Configuration.Installers;

public class RateLimiterInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => _ = webApplicationBuilder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        _ = options.AddPolicy(AppConstants.RateLimiting.AnonymusRestricted, httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                factory: partition => new SlidingWindowRateLimiterOptions
                {
                    SegmentsPerWindow = 6,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });
}
