using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Sechat.Data;
using System;
using System.Text;

namespace Sechat.Service.Configuration.Installers;

public class AuthServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        var configuration = webApplicationBuilder.Configuration;

        _ = webApplicationBuilder.Services.AddDataProtection()
                    .SetApplicationName(webApplicationBuilder.Configuration.GetValue("AppSettings:Name", ""))
                    .PersistKeysToDbContext<SechatContext>();

        if (webApplicationBuilder.Environment.IsProduction())
        {
            _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 8;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<SechatContext>()
            .AddDefaultTokenProviders();
        }

        if (webApplicationBuilder.Environment.IsDevelopment() || webApplicationBuilder.Environment.EnvironmentName.Equals(AppConstants.CustomEnvironment.Test))
        {
            _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 50;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(5);
            })
            .AddEntityFrameworkStores<SechatContext>()
            .AddDefaultTokenProviders();
        }

        _ = webApplicationBuilder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
        {
            var secretBytes = Encoding.UTF8.GetBytes(configuration.GetValue("JwtOptions:SecretKey", ""));
            var key = new SymmetricSecurityKey(secretBytes);

            if (webApplicationBuilder.Environment.IsDevelopment())
            {
                config.RequireHttpsMetadata = false;
            }

            config.SaveToken = true;
            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = configuration.GetValue("JwtOptions:Issuer", ""),
                ValidAudience = configuration.GetValue("JwtOptions:Audience", ""),
                IssuerSigningKey = key,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };
        });

        _ = webApplicationBuilder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        _ = webApplicationBuilder.Services.ConfigureApplicationCookie(config =>
        {
            config.Cookie.Domain = webApplicationBuilder.Configuration.GetValue("CookieSettings:AuthCookieDomain", "");
            config.Cookie.Name = webApplicationBuilder.Configuration.GetValue("CookieSettings:AuthCookieName", "");
            config.ExpireTimeSpan = TimeSpan.FromDays(30);
            config.Cookie.SameSite = SameSiteMode.Lax;
        });

        _ = webApplicationBuilder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, IdentityConstants.ApplicationScheme)
                .Build();

            options.AddPolicy(AppConstants.AuthorizationPolicy.TokenPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

            options.AddPolicy(AppConstants.AuthorizationPolicy.AdminPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(AppConstants.ClaimType.Role,
                    AppConstants.Role.Admin));

            options.AddPolicy(AppConstants.AuthorizationPolicy.ChatPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(AppConstants.ClaimType.ServiceClaim,
                    AppConstants.ServiceClaimValue.ChatAccess));
        });

        _ = webApplicationBuilder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.Zero;
        });
    }
}
