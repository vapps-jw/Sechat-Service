using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Settings;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebPush;

namespace Sechat.Service.Services;

public class PushNotificationService
{
    private readonly UserRepository _userRepository;
    private readonly IOptions<VapidKeys> _vapidKeys;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(UserRepository userRepository, IOptions<VapidKeys> vapidKeys, ILogger<PushNotificationService> logger)
    {
        _userRepository = userRepository;
        _vapidKeys = vapidKeys;
        _logger = logger;
    }

    public async Task IncomingMessageNotification(string userId, string roomName)
    {
        var subs = _userRepository.GetSubscriptions(userId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails("mailto:office@vapps.pl", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = "New Message",
                    options = new
                    {
                        body = roomName
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                _logger.LogError(exception, exception.Message);
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
            }
        }
    }

    public async Task IncomingContactRequestNotification(string userId, string inviterName)
    {
        var subs = _userRepository.GetSubscriptions(userId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails("mailto:office@vapps.pl", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = "New Invitation",
                    options = new
                    {
                        body = $"Contact request from {inviterName}"
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                _logger.LogError(exception, exception.Message);
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
            }
        }
    }
}
