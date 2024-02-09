using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sechat.Data;
using Sechat.Service.Settings;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Sechat.Service.Configuration.Installers;

//public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
//{
//    private readonly IHostEnvironment _hostEnvironment;
//    private readonly IOptionsMonitor<TokenSettings> _tokenSettings;

//    public ConfigureJwtBearerOptions(
//        IHostEnvironment hostEnvironment,
//        IOptionsMonitor<TokenSettings> tokenSettings)
//    {
//        _hostEnvironment = hostEnvironment;
//        _tokenSettings = tokenSettings;
//    }

//    public void Configure(JwtBearerOptions options)
//    {
//        // I decided to throw an exception here.
//    }

//    public void Configure(string name, JwtBearerOptions options)
//    {

//        //var token = "[encoded jwt]";
//        //var handler = new JwtSecurityTokenHandler();
//        //var jwtSecurityToken = handler.ReadJwtToken(token);

//        var rsa = RSA.Create();
//        rsa.ImportFromPem("");
//        var key = new RsaSecurityKey(rsa);

//        if (_hostEnvironment.IsDevelopment())
//        {
//            options.RequireHttpsMetadata = false;
//        }

//        options.SaveToken = true;
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidIssuer = _tokenSettings.CurrentValue.Issuer,
//            ValidAudience = _tokenSettings.CurrentValue.Audience,
//            IssuerSigningKey = key,
//            ValidateIssuerSigningKey = true,
//            ClockSkew = TimeSpan.Zero,
//            ValidateIssuer = true,
//            ValidateAudience = true
//        };
//    }
//}

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
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
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

        //_ = webApplicationBuilder.Services.AddAuthentication().AddJwtBearer("jwt", options =>
        //{
        //    var secretBytes = Encoding.UTF8.GetBytes(configuration.GetValue("AuthSettings:SecretKey", ""));
        //    var key = new SymmetricSecurityKey(secretBytes);

        //    if (webApplicationBuilder.Environment.IsDevelopment())
        //    {
        //        options.RequireHttpsMetadata = false;
        //    }

        //    options.SaveToken = true;
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidIssuer = configuration.GetValue("AuthSettings:Issuer", ""),
        //        ValidAudience = configuration.GetValue("AuthSettings:Audience", ""),
        //        IssuerSigningKey = key,
        //        ValidateIssuerSigningKey = true,
        //        ClockSkew = TimeSpan.Zero,
        //        ValidateIssuer = true,
        //        ValidateAudience = true
        //    };

        //});

        _ = webApplicationBuilder.Services.ConfigureApplicationCookie(config =>
        {
            config.Cookie.Domain = webApplicationBuilder.Configuration.GetValue("CookieSettings:AuthCookieDomain", "");
            config.Cookie.Name = webApplicationBuilder.Configuration.GetValue("CookieSettings:AuthCookieName", "");
            config.ExpireTimeSpan = TimeSpan.FromDays(30);
            config.Cookie.SameSite = SameSiteMode.Lax;
        });

        _ = webApplicationBuilder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        _ = webApplicationBuilder.Services.AddAuthorization(options =>
        {
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
