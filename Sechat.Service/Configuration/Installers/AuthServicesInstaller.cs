using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sechat.Data;
using Sechat.Service.Utilities;
using System;

namespace Sechat.Service.Configuration.Installers;

public class AuthServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddDataProtection()
                    .SetApplicationName(webApplicationBuilder.Configuration.GetValue("AppSettings:Name", ""))
                    .PersistKeysToDbContext<SechatContext>();

        if (webApplicationBuilder.Environment.IsProduction())
        {
            _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<SechatContext>()
            .AddDefaultTokenProviders();
        }

        if (webApplicationBuilder.Environment.IsDevelopment() || webApplicationBuilder.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironments.TestEnv))
        {
            _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<SechatContext>()
            .AddDefaultTokenProviders();
        }

        _ = webApplicationBuilder.Services.ConfigureApplicationCookie(config =>
        {
            config.Cookie.Domain = webApplicationBuilder.Configuration.GetValue("CorsSettings:CookieDomain", "");
            config.Cookie.Name = "sechat-id";
            config.ExpireTimeSpan = TimeSpan.FromDays(30);
            config.Cookie.SameSite = SameSiteMode.Lax;
        });

        _ = webApplicationBuilder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
