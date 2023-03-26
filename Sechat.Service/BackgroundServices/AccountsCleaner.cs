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
    private int _cleanInterval = 60;

    public AccountsCleaner(ILogger<AccountsCleaner> logger, IDbContextFactory<SechatContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(_cleanInterval), CancellationToken.None);
            try
            {
                _logger.LogWarning("Accounts Cleanup Started");
                using var ctx = await _contextFactory.CreateDbContextAsync(stoppingToken);
                ctx.UserProfiles.RemoveRange(ctx.UserProfiles.Where(p => p.LastActivity <= DateTime.UtcNow.AddDays(-30)));
                _ = await ctx.SaveChangesAsync(stoppingToken);
                _cleanInterval = 60;
            }
            catch (Exception ex)
            {
                _cleanInterval = 1;
                _exceptionCount++;
                _logger.LogError(ex, "Message Cleaner Exception no. {exceptionCount}", _exceptionCount);
            }
        }
    }
}
