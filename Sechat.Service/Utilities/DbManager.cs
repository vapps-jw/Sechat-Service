using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Utilities;

public static class DbManager
{
    public static void PrepareDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<SechatContext>();
        if (context is not null)
        {
            ApplyMigrations(context);
        }
    }

    private static void ApplyMigrations(SechatContext context)
    {
        Console.WriteLine("--> Attempting to apply migrations...");
        try
        {
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not run migrations: {ex.Message}");
        }
    }

    public static async Task SeedData(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var userManager = serviceScope.ServiceProvider.GetService<UserManager<IdentityUser>>();

        for (var i = 1; i < 11; i++)
        {
            var text = $"u{i}";
            var user = new IdentityUser(text);
            var res = await userManager.CreateAsync(user, text);
            Console.WriteLine($"--> Creating User: {text} - Success: {res.Succeeded}");
        }
    }
}
