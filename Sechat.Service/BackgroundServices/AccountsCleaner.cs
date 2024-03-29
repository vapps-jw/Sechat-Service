﻿using Microsoft.EntityFrameworkCore;
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

    public AccountsCleaner(ILogger<AccountsCleaner> logger, IDbContextFactory<SechatContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pt = new PeriodicTimer(TimeSpan.FromDays(1));
            _ = await pt.WaitForNextTickAsync();
            try
            {
                using var ctx = await _contextFactory.CreateDbContextAsync(stoppingToken);

                var profilesToDelete = ctx.UserProfiles.Where(p => p.LastActivity <= DateTime.UtcNow.AddMonths(-12)).ToList();
                if (!profilesToDelete.Any()) continue;

                var ids = profilesToDelete.Select(p => p.Id).ToList();
                var roomsToDelete = ctx.Rooms.Where(r => ids.Contains(r.CreatorId)).ToList();
                var contactsToDelete = ctx.Contacts.Where(uc => ids.Contains(uc.InvitedId) || ids.Contains(uc.InviterId));

                ctx.UserProfiles.RemoveRange(profilesToDelete);
                ctx.Rooms.RemoveRange(roomsToDelete);
                ctx.Contacts.RemoveRange(contactsToDelete);

                var res = await ctx.SaveChangesAsync(stoppingToken);
                _logger.LogWarning("Accounts cleanup, deleted records: {records}", res);
            }
            catch (Exception ex)
            {
                _exceptionCount++;
                _logger.LogError(ex, "Message Cleaner Exception no. {exceptionCount}", _exceptionCount);
            }
        }
    }
}
