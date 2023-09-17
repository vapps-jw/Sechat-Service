using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sechat.Data;
using Sechat.Service.Configuration;
using Sechat.Service.Settings;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sechat.Service.Utilities;

public static class UsersSetup
{
    public static async Task PrepareAdmins(IApplicationBuilder app)
    {
        Console.WriteLine("--> Checking Admins...");
        using var serviceScope = app.ApplicationServices.CreateScope();

        var email = serviceScope.ServiceProvider.GetService<IOptions<SechatEmails>>();
        var context = serviceScope.ServiceProvider.GetService<SechatContext>();
        var userManager = serviceScope.ServiceProvider.GetService<UserManager<IdentityUser>>();

        var admin = await userManager.FindByEmailAsync(email.Value.Master);
        if (admin is null) return;

        var claims = await userManager.GetClaimsAsync(admin);
        if (!claims.Any(c => c.Type.Equals(AppConstants.ClaimType.Role) && c.Value.Equals(AppConstants.Role.Admin)))
        {
            var resut = await userManager.AddClaimAsync(admin, new Claim(AppConstants.ClaimType.Role, AppConstants.Role.Admin));
            if (resut.Succeeded)
            {
                Console.WriteLine($"--> Admin Added {admin.UserName}");
            }
            else
            {
                Console.WriteLine($"--> Admin Not Added {admin.UserName}");
            }
        }
    }
}
