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

    public PushNotificationService(UserRepository userRepository, IOptions<VapidKeys> vapidKeys)
    {
        _userRepository = userRepository;
        _vapidKeys = vapidKeys;
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
                    title = roomName,
                    options = new
                    {
                        body = "New Message"
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                Console.WriteLine("Http STATUS code" + exception.StatusCode);
            }
        }
    }
}
