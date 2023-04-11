using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.BackgroundServices;

public class AccountsCleaner : BackgroundService
{
    private readonly ILogger<AccountsCleaner> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private int _exceptionCount;
    private readonly int _cleanInterval = 1;

    public AccountsCleaner(ILogger<AccountsCleaner> logger, IDbContextFactory<SechatContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromDays(_cleanInterval), CancellationToken.None);
            try
            {
                _logger.LogWarning("Accounts Cleanup Started");
                using var ctx = await _contextFactory.CreateDbContextAsync(stoppingToken);

                var profilesToDelete = ctx.UserProfiles.Where(p => p.LastActivity <= DateTime.UtcNow.AddDays(-30)).ToList();
                if (!profilesToDelete.Any()) continue;

                var ids = profilesToDelete.Select(p => p.Id).ToList();
                var roomsToDelete = ctx.Rooms.Where(r => ids.Contains(r.CreatorId)).ToList();

                ctx.UserProfiles.RemoveRange(profilesToDelete);
                ctx.Rooms.RemoveRange(roomsToDelete);
                _ = await ctx.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _exceptionCount++;
                _logger.LogError(ex, "Message Cleaner Exception no. {exceptionCount}", _exceptionCount);
            }
        }
    }
}
