﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;

namespace Sechat.Service.Configuration
{
    public class DataServiceInstaller : IServiceInstaller
    {
        public void Install(WebApplicationBuilder webApplicationBuilder)
        {
            _ = webApplicationBuilder.Services.AddDbContext<SechatContext>(options =>
                options.UseNpgsql(webApplicationBuilder.Configuration.GetConnectionString("Master"),
                serverAction =>
                {
                    _ = serverAction.EnableRetryOnFailure(3);
                    _ = serverAction.CommandTimeout(20);
                }));
            _ = webApplicationBuilder.Services.AddDataProtection()
                        .SetApplicationName("vapps")
                        .PersistKeysToDbContext<SechatContext>();
            _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>()
                        .AddEntityFrameworkStores<SechatContext>()
                        .AddDefaultTokenProviders();
        }
    }
}
