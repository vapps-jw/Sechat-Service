namespace Sechat.Service.Configuration;

public class AppConstants
{
    public struct Paths
    {
        public const string SecretSettings = @"secrets/appsettings.secrets.json";
    }
    public struct CustomEnvironments
    {
        public const string Test = nameof(Test);
    }
    public struct PushNotificationTitles
    {
        public const string NewMessage = "New Message";
        public const string NewDirectMessage = "Direct Message";
        public const string NewInvitation = "New Invitation";
        public const string InvitationApproved = "Invitation Approved";
        public const string EventReminder = "Event Reminder";
        public const string VideoCall = "Video Call";
    }
    public struct CacheProfiles
    {
        public const string NoCache = "NoCache";
    }
    public struct ContactState
    {
        public const string Online = "Online";
        public const string Offline = "Offline";
        public const string Unknown = "Unknown";
    }
    public struct KeyTypes
    {
        public const string E2E_MASTER = "E2E_MASTER";
        public const string E2E_ROOMS = "E2E_ROOMS";
        public const string E2E_DM = "E2E_DM";
    }
    public enum PushNotificationType
    {
        IncomingVideoCall,
        IncomingMessage,
        IncomingDirectMessage,
        IncomingContactRequest,
        ContactRequestApproved,
    }
    public struct ContentTypes
    {
        public const string Json = "application/json";
    }
    public struct CorsPolicies
    {
        public const string WebClient = nameof(WebClient);
    }
    public struct RateLimiting
    {
        public const string AnonymusRestricted = nameof(AnonymusRestricted);
    }
    public struct StringLengths
    {
        public const int NameMax = 50;
        public const int TextMax = 3000;
        public const int PasswordMax = 20;
        public const int PasswordMin = 8;
        public const int UserNameMax = 10;
        public const int UserNameMin = 3;
    }
}
