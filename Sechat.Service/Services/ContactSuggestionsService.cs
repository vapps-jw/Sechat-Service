using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sechat.Data;
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

    public async Task<List<ContactSuggestion>> CreateContactSuggections(string askingUser, List<string> suggested, CancellationToken cancellationToken)
    {
        await _cacheUpdateSemaphore.WaitAsync(cancellationToken);

        try
        {
            var excluded = new List<string>(suggested);
            if (!excluded.Contains(askingUser)) excluded.Add(askingUser);

            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var askingUserNode = new ContactSuggestion()
            {
                UserName = askingUser,
            };

            if (!_cacheService.Cache.IsCached(new ContactSuggestion() { UserName = askingUser }))
            {
                askingUserNode.ProfilePicture = ctx.UserProfiles
                    .Where(up => up.UserName.Equals(askingUser))
                    .Select(up => up.ProfilePicture)
                    .FirstOrDefault();

                var suggestions = await GetSuggestions(askingUser, ctx);
                _cacheService.Cache.Update(askingUserNode, suggestions);
            }

            var result = new List<ContactSuggestion>();

            var edges = _cacheService.Cache.GetEdges(askingUserNode);
            excluded = excluded
                .Concat(edges.Select(e => e.UserName))
                .Distinct()
                .ToList();

            var checkQueue = new Queue<ContactSuggestion>();
            edges.ForEach(checkQueue.Enqueue);

            while (result.Count <= 20 && checkQueue.Any())
            {
                var node = checkQueue.Dequeue();
                if (!_cacheService.Cache.IsCached(node))
                {
                    _cacheService.Cache.Update(node, await GetSuggestions(node.UserName, ctx));
                }

                var suggestions = _cacheService.Cache.GetEdges(node);
                if (!suggestions.Any())
                {
                    _cacheService.Cache.Update(node, await GetSuggestions(node.UserName, ctx));
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

    private Task<List<ContactSuggestion>> GetSuggestions(string root, SechatContext ctx)
    {
        return ctx.Contacts
            .Where(uc => uc.InvitedName.Equals(root) || uc.InviterName.Equals(root))
            .Select(c => c.InvitedName.Equals(root) ? c.InviterName : c.InvitedName)
            .Select(c => new ContactSuggestion()
            {
                UserName = c,
                ProfilePicture = ctx.UserProfiles.Where(up => up.UserName.Equals(c)).Select(up => up.ProfilePicture).FirstOrDefault()
            })
            .ToListAsync();
    }
}

public record ContactSuggestion
{
    public string UserName { get; set; }
    public string ProfilePicture { get; set; }

    public override string ToString() => UserName;
}
