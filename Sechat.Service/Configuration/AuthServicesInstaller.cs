using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;

namespace Sechat.Service.Configuration;

public class AuthServicesInstaller : IServiceInstaller
{
    public void Install(WebApplicationBuilder webApplicationBuilder)
    {
        _ = webApplicationBuilder.Services.AddDataProtection()
                    .SetApplicationName(webApplicationBuilder.Configuration.GetValue("AppSettings:Name", ""))
                    .PersistKeysToDbContext<SechatContext>();
        _ = webApplicationBuilder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = false;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<SechatContext>()
        .AddDefaultTokenProviders();

        _ = webApplicationBuilder.Services.ConfigureApplicationCookie(config =>
        {
            config.Cookie.Domain = webApplicationBuilder.Configuration.GetValue("CorsSettings:CookieDomain", "");
        });

        _ = webApplicationBuilder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
