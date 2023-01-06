using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Sechat.Service.Utilities;
using System;

namespace Sechat.Service.Configuration;

public class RateLimiterInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder) => webApplicationBuilder.Services.AddRateLimiter(options =>
    {
        _ = webApplicationBuilder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
        });

        _ = options.AddSlidingWindowLimiter(AppConstants.RateLimiting.DefaultWindowPolicyName, options =>
        {
            options.AutoReplenishment = true;
            options.SegmentsPerWindow = 6;
            options.PermitLimit = 6000;
            options.Window = TimeSpan.FromMinutes(1);
        });
    });
}
