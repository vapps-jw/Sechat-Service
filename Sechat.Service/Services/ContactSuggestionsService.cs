using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Data.SechatLinqExtensions;
using Sechat.Service.Services.CacheServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

// todo: finish cs service
public class ContactSuggestionsService
{
    private readonly SemaphoreSlim _cacheUpdateSemaphore;
    private readonly ILogger<ContactSuggestionsService> _logger;
    private readonly ContactSuggestionsCache _cacheService;
    private readonly IDbContextFactory<SechatContext> _contextFactory;

    public ContactSuggestionsService(
        ILogger<ContactSuggestionsService> logger,
        ContactSuggestionsCache cacheService,
        IDbContextFactory<SechatContext> contextFactory)
    {
        _cacheUpdateSemaphore = new SemaphoreSlim(1);
        _logger = logger;
        _cacheService = cacheService;
        _contextFactory = contextFactory;
    }

    public async Task<List<ContactSuggestion>> CreateSuggectionsList(string askingUser, List<string> suggested, CancellationToken cancellationToken = default)
    {
        await _cacheUpdateSemaphore.WaitAsync(cancellationToken);

        try
        {
            var excluded = new List<string>(suggested);
            if (!excluded.Contains(askingUser)) excluded.Add(askingUser);

            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var askingUserNode = new ContactSuggestion(askingUser);

            if (!_cacheService.Cache.IsCached(new ContactSuggestion(askingUser)))
            {
                askingUserNode.ProfilePicture = ctx.UserProfiles.GetProfilePicture(askingUser);

                var userEdges = await GetSuggestions(askingUser);
                _cacheService.Cache.Update(askingUserNode, userEdges);
            }

            var result = new List<ContactSuggestion>();
            var edges = _cacheService.Cache.GetEdges(askingUserNode);
            excluded = excluded
                .Concat(edges.Select(e => e.UserName))
                .Distinct()
                .ToList();

            var checkQueue = new Queue<ContactSuggestion>();
            edges.ForEach(checkQueue.Enqueue);

            while (result.Count <= 10 && checkQueue.Any())
            {
                var node = checkQueue.Dequeue();
                if (!_cacheService.Cache.IsCached(node))
                {
                    _cacheService.Cache.Update(node, await GetSuggestions(node.UserName, default));
                }

                var suggestions = _cacheService.Cache.GetEdges(node);
                if (!suggestions.Any())
                {
                    _cacheService.Cache.Update(node, await GetSuggestions(node.UserName, default));
                }

                suggestions = _cacheService.Cache.GetEdges(node);
                var newSuggestions = suggestions
                    .Where(s => !excluded.Contains(s.UserName))
                    .ToList();

                if (newSuggestions.Any())
                {
                    excluded.AddRange(newSuggestions.Select(s => s.UserName));
                    result.AddRange(newSuggestions);
                    newSuggestions.ForEach(checkQueue.Enqueue);
                }

                continue;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _ = _cacheUpdateSemaphore.Release();
        }
    }

    private async Task<List<ContactSuggestion>> GetSuggestions(string root, CancellationToken cancellationToken = default)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Contacts
            .GetContacts(root)
            .Where(uc => !uc.Blocked)
            .Select(c => c.InvitedName.Equals(root) ? c.InviterName : c.InvitedName)
            .Select(c => new ContactSuggestion(c)
            {
                ProfilePicture = ctx.UserProfiles.Where(up => up.UserName.Equals(c)).Select(up => up.ProfilePicture).FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task UpdateCache(string askingUser, CancellationToken cancellationToken = default)
    {
        await _cacheUpdateSemaphore.WaitAsync(cancellationToken);
        try
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken = default);
            var askingUserNode = new ContactSuggestion(askingUser)
            {
                ProfilePicture = ctx.UserProfiles.GetProfilePicture(askingUser)
            };

            var suggestions = await GetSuggestions(askingUser, cancellationToken);
            _cacheService.Cache.Update(askingUserNode, suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _ = _cacheUpdateSemaphore.Release();
        }
    }

    public async Task DeleteContact(string askingUser, CancellationToken cancellationToken = default)
    {
        await _cacheUpdateSemaphore.WaitAsync(cancellationToken);
        try
        {
            _cacheService.Cache.Delete(new ContactSuggestion(askingUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _ = _cacheUpdateSemaphore.Release();
        }
    }
}

public class ContactSuggestion
{
    public string UserName { get; init; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;

    public ContactSuggestion(string key) => UserName = key;

    public override string ToString() => UserName;

    public override bool Equals(object obj) =>
        obj is ContactSuggestion other && UserName.Equals(other.UserName);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 23) + (UserName ?? "").GetHashCode();
            return hash;
        }
    }
}

