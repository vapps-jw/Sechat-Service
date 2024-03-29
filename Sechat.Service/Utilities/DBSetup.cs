﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Utilities;

public static class DBSetup
{
    public static void PrepareDatabase(IApplicationBuilder app)
    {
        Console.WriteLine("--> Preparing DB...");
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<SechatContext>();

        if (context is not null)
        {
            ApplyMigrations(context);
        }
    }

    public static void EnsureCreatedDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<SechatContext>();
        _ = (context?.Database.EnsureCreated());
    }

    private static void ApplyMigrations(SechatContext context)
    {
        Console.WriteLine("--> Attempting to apply migrations...");
        try
        {
            if (!context.Database.GetPendingMigrations().Any())
            {
                Console.WriteLine("--> No new migrations...");
                return;
            }

            Console.WriteLine("--> Applying migrations...");
            context.Database.Migrate();
            Console.WriteLine("--> Migrations applied...");
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

        var repo = serviceScope.ServiceProvider.GetService<UserRepository>();

        if (repo.CountUserProfiles() > 0) return;

        for (var i = 1; i < 31; i++)
        {
            var name = $"u{i}";
            var user = new IdentityUser(name);
            var res = await userManager.CreateAsync(user, name);
            if (res.Succeeded)
            {
                repo.CreateUserProfile(user.Id, user.UserName);
            }

            Console.WriteLine($"--> Creating User: {name} - Success: {res.Succeeded}");
        }

        var ctxFactoryScope = serviceScope.ServiceProvider.GetRequiredService<IDbContextFactory<SechatContext>>();
        var ctx = ctxFactoryScope.CreateDbContext();
        var users = ctx.UserProfiles.ToList();

        var level = 1;
        for (var i = 0; i < users.Count; i++)
        {
            for (var j = 1; j <= level && i + j <= users.Count - 1; j++)
            {
                _ = await CreateContact(users[i], users[i + j], ctx);
            }
            level += level;
        }
    }

    private static async Task<Contact> CreateContact(UserProfile inviter, UserProfile invited, SechatContext context)
    {
        var result = new Contact()
        {
            Approved = true,
            InvitedId = invited.Id,
            InvitedName = invited.UserName,
            InviterId = inviter.Id,
            InviterName = inviter.UserName,
        };
        _ = context.Contacts.Add(result);
        _ = await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return result;
    }
}
