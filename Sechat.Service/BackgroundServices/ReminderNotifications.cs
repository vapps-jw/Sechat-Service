using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sechat.Data;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.BackgroundServices;

public class ReminderNotifications : BackgroundService
{
    private readonly Channel<DefaultNotificationDto> _channel;
    private readonly ILogger<ReminderNotifications> _logger;
    private readonly IDbContextFactory<SechatContext> _contextFactory;

    public ReminderNotifications(
        Channel<DefaultNotificationDto> channel,
        ILogger<ReminderNotifications> logger,
        IDbContextFactory<SechatContext> contextFactory)
    {
        _channel = channel;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pt = new PeriodicTimer(TimeSpan.FromSeconds(5));
            _ = await pt.WaitForNextTickAsync();
            try
            {
                using var ctx = await _contextFactory.CreateDbContextAsync(stoppingToken);
                var reminders = ctx.Reminders
                    .Where(r => r.Reminded == 0 && r.Remind <= DateTime.UtcNow)
                    .Include(r => r.CalendarEvent)
                    .ThenInclude(ce => ce.Calendar)
                    .ToList();

                if (!reminders.Any()) continue;
                foreach (var reminder in reminders)
                {
                    await _channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.EventReminder, reminder.CalendarEvent.Calendar.UserProfileId, reminder.CalendarEvent.Data));
                    reminder.Reminded += 1;
                }
                _ = await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Automated Notifications Exception {exm}", ex.Message);
            }
        }
    }
}
