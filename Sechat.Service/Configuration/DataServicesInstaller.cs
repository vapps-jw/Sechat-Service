using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sechat.Data;
using System;

namespace Sechat.Service.Configuration;

public class DataServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            _ = webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
                 options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        }

        if (webApplicationBuilder.Environment.IsProduction())
        {
            _ = webApplicationBuilder.Services.AddDbContextFactory<SechatContext>(options =>
                options.UseNpgsql(webApplicationBuilder.Configuration.GetConnectionString("Master"),
                serverAction =>
                {
                    _ = serverAction.EnableRetryOnFailure(3);
                    _ = serverAction.CommandTimeout(20);
                }));
        }
    }
}
