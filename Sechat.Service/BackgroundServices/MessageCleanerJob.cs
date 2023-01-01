using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sechat.Service.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.BackgroundServices;

public class MessageCleanerJob : CronJobService
{
    private readonly ILogger<MessageCleanerJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MessageCleanerJob(IScheduleConfig<MessageCleanerJob> config, ILogger<MessageCleanerJob> logger, IServiceProvider serviceProvider)
        : base(config.CronExpression, config.TimeZoneInfo)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Message Cleaner Job starts");
        return base.StartAsync(cancellationToken);
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Message Cleaner Job is executing");
        using var scope = _serviceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<MessageCleaner>();
        await svc.DoWork(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message Cleaner Job is stopping");
        return base.StopAsync(cancellationToken);
    }
}
