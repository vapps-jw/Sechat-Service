﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data;
using Sechat.Data.Models.UserDetails;
using Sechat.Service.Services;
using Sechat.Service.Services.CacheServices;
using Sechat.Tests.Utils;
using System.Diagnostics;

namespace Sechat.Tests;
public class ContactSuggestionsTests
{
    //private async Task<Contact> CreateContact(UserProfile inviter, UserProfile invited, SechatContext context)
    //{
    //    var result = new Contact()
    //    {
    //        Approved = true,
    //        InvitedId = invited.Id,
    //        InvitedName = invited.UserName,
    //        InviterId = inviter.Id,
    //        InviterName = inviter.UserName,
    //    };
    //    _ = context.Contacts.Add(result);
    //    _ = await context.SaveChangesAsync();
    //    context.ChangeTracker.Clear();
    //    return result;
    //}

    //[Fact]
    //public async Task SuggestContacts()
    //{
    //    using var masterApp = new MockedApi();
    //    using var scope = masterApp.Services.CreateScope();

    //    var ctxFactoryScope = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SechatContext>>();
    //    var ctx = ctxFactoryScope.CreateDbContext();

    //    var contactsService = scope.ServiceProvider.GetRequiredService<ContactSuggestionsService>();
    //    var cache = scope.ServiceProvider.GetRequiredService<ContactSuggestionsCache>();

    //    var users = ctx.UserProfiles.ToList();
    //    Assert.NotEmpty(users);

    //    var level = 1;
    //    for (var i = 0; i < users.Count; i++)
    //    {
    //        for (var j = 1; j <= level && i + j <= users.Count - 1; j++)
    //        {
    //            _ = await CreateContact(users[i], users[i + j], ctx);
    //        }
    //        level += level;
    //    }

    //    var contacts = ctx.Contacts.ToList();
    //    Assert.NotEmpty(contacts);

    //    var res = await contactsService.CreateSuggectionsList("u1", new List<string>(), default);
    //    cache.Cache.Print();

    //    Assert.NotEmpty(res);
    //    Assert.Equal(7, res.Count);

    //    Debug.WriteLine("-------------");
    //    var csToDelete = new ContactSuggestion("u5");
    //    await contactsService.DeleteContact("u5", default);
    //    cache.Cache.Print();

    //    Assert.DoesNotContain(csToDelete, cache.Cache.Matrix.SelectMany(i => i.Value));

    //    res = await contactsService.CreateSuggectionsList("u1", new List<string>(), default);
    //    Assert.Equal(6, res.Count);
    //}

    //[Fact]
    //public async Task CacheUpdates()
    //{
    //    using var masterApp = new MockedApi();
    //    using var scope = masterApp.Services.CreateScope();

    //    var ctxFactoryScope = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SechatContext>>();
    //    var ctx = ctxFactoryScope.CreateDbContext();

    //    var contactsService = scope.ServiceProvider.GetRequiredService<ContactSuggestionsService>();
    //    var cache = scope.ServiceProvider.GetRequiredService<ContactSuggestionsCache>();

    //    var users = ctx.UserProfiles.ToList();
    //    Assert.NotEmpty(users);

    //    var level = 1;
    //    for (var i = 0; i < users.Count; i++)
    //    {
    //        for (var j = 1; j <= level && i + j <= users.Count - 1; j++)
    //        {
    //            _ = await CreateContact(users[i], users[i + j], ctx);
    //        }
    //        level += level;
    //    }

    //    var contacts = ctx.Contacts.ToList();
    //    Assert.NotEmpty(contacts);

    //    foreach (var user in users)
    //    {
    //        await contactsService.UpdateCache(user.UserName, default);
    //    }

    //    cache.Cache.Print();
    //    Assert.Equal(9, cache.Cache.Matrix.Count);

    //    await contactsService.DeleteContact("u3", default);
    //    Assert.Equal(8, cache.Cache.Matrix.Count);

    //}
}
