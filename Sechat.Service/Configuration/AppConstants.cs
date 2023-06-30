namespace Sechat.Service.Configuration;

public class AppConstants
{
    public struct Paths
    {
        public const string SecretSettings = @"secrets/appsettings.secrets.json";
    }
    public struct CustomEnvironments
    {
        public const string TestEnv = nameof(TestEnv);
    }
    public struct PushNotificationTitles
    {
        public const string NewMessage = "New Message";
        public const string NewInvitation = "New Invitation";
        public const string InvitationApproved = "Invitation Approved";
        public const string VideoCall = "Video Call";
    }
    public struct ContactState
    {
        public const string Online = "Online";
        public const string Offline = "Offline";
        public const string Unknown = "Unknown";
    }
    public struct Cookies
    {
        public const string E2E = "E2E";
    }
    public enum PushNotificationType
    {
        IncomingVideoCall,
        IncomingMessage,
        IncomingContactRequest,
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
        public const string MinimalRateLimiterPolicy = nameof(MinimalRateLimiterPolicy);
    }
    public struct StringLengths
    {
        public const int PasswordMax = 20;
        public const int PasswordMin = 8;
        public const int UserNameMax = 10;
        public const int UserNameMin = 3;
    }
}
