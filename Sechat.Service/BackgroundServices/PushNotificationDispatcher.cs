using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sechat.Service.Dtos;
using Sechat.Service.Services;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.BackgroundServices;

public class PushNotificationDispatcher : BackgroundService
{
    private readonly Channel<DefaultNotificationDto> _channel;
    private readonly ILogger<PushNotificationDispatcher> _logger;
    private readonly IServiceProvider _provider;

    public PushNotificationDispatcher(
        Channel<DefaultNotificationDto> channel,
        ILogger<PushNotificationDispatcher> logger,
        IServiceProvider provider)
    {
        _channel = channel;
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!_channel.Reader.Completion.IsCompleted)
        {
            var msg = await _channel.Reader.ReadAsync(stoppingToken);
            try
            {
                using var scope = _provider.CreateScope();
                var ns = scope.ServiceProvider.GetRequiredService<PushNotificationService>();

                switch (msg.NotificationType)
                {
                    case Configuration.AppConstants.PushNotificationType.IncomingVideoCall:
                        await ns.IncomingVideoCallNotification(msg.UserId, msg.BodyData);
                        break;
                    case Configuration.AppConstants.PushNotificationType.IncomingMessage:
                        await ns.IncomingMessageNotification(msg.UserId, msg.BodyData);
                        break;
                    case Configuration.AppConstants.PushNotificationType.IncomingContactRequest:
                        await ns.IncomingContactRequestNotification(msg.UserId, msg.BodyData);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Push notification service failed");
            }
        }
    }
}
