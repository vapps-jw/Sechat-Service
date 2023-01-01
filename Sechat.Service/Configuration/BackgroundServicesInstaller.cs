using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Service.BackgroundServices;
using Sechat.Service.Services;
using System;

namespace Sechat.Service.Configuration;

public class BackgroundServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddScoped<MessageCleaner>();
        _ = webApplicationBuilder.Services.AddCronJob<MessageCleanerJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = @"* * * * *";
        });
    }
}
