﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.BackgroundServices;

public class MessageCleaner : BackgroundService
{
    private readonly ILogger<MessageCleaner> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private int _exceptionCount;
    private int _cleanInterval = 60;

    public MessageCleaner(ILogger<MessageCleaner> logger, IDbContextFactory<SechatContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(_cleanInterval), CancellationToken.None);
            try
            {

                _cleanInterval = 60;
            }
            catch (Exception ex)
            {
                _cleanInterval = 10;
                _exceptionCount++;
                _logger.LogError(ex, "Message Cleaner Exception no. {0}", _exceptionCount);
            }
        }
    }
}
