using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
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
    private readonly IOptions<SechatEmails> _sechatEmails;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        UserRepository userRepository,
        IOptions<VapidKeys> vapidKeys,
        IOptions<SechatEmails> sechatEmails,
        ILogger<PushNotificationService> logger)
    {
        _userRepository = userRepository;
        _vapidKeys = vapidKeys;
        _sechatEmails = sechatEmails;
        _logger = logger;
    }

    private async Task RemoveInvalidSubscription(int subId)
    {
        _logger.LogWarning("Removing invalid subscription");
        _userRepository.RemovePushNotificationSubscription(subId);
        _ = await _userRepository.SaveChanges();
    }

    public async Task IncomingVideoCallNotification(string recipientId, string callerName)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.VideoCall,
                    options = new
                    {
                        body = $"Call from {callerName}"
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    public async Task IncomingMessageNotification(string recipientId, string roomName)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.NewMessage,
                    options = new
                    {
                        body = roomName
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    public async Task IncomingDirectMessageNotification(string recipientId, string senderName)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.NewDirectMessage,
                    options = new
                    {
                        body = senderName
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    public async Task IncomingContactRequestNotification(string recipientId, string inviterName)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.NewInvitation,
                    options = new
                    {
                        body = $"Contact request from {inviterName}"
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    public async Task ContactRequestApprovedNotification(string recipientId, string approverName)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.InvitationApproved,
                    options = new
                    {
                        body = $"{approverName} accepted your invitation"
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }

    public async Task EventReminderNotification(string recipientId, string message)
    {
        var subs = _userRepository.GetSubscriptions(recipientId);
        if (!subs.Any()) return;

        foreach (var sub in subs)
        {
            var subscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
            var vapidDetails = new VapidDetails($"mailto:{_sechatEmails.Value.Master}", _vapidKeys.Value.PublicKey, _vapidKeys.Value.PrivateKey);

            var webPushClient = new WebPushClient();
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    title = AppConstants.PushNotificationTitles.EventReminder,
                    options = new
                    {
                        body = message
                    }
                });

                await webPushClient.SendNotificationAsync(subscription, payload, vapidDetails);
            }
            catch (WebPushException exception)
            {
                if (exception.Message.Contains("Subscription no longer valid", StringComparison.InvariantCultureIgnoreCase))
                {
                    await RemoveInvalidSubscription(sub.Id);
                    return;
                }
                _logger.LogError(exception, exception.Message);
            }
        }
    }
}
