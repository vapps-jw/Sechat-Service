using static Sechat.Service.Configuration.AppConstants;

namespace Sechat.Service.Dtos;

public record DefaultNotificationDto(PushNotificationType NotificationType, string UserId, string BodyData);
