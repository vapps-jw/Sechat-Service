namespace Sechat.Service.Configuration;

public class AppConstants
{
    public struct Files
    {
        public const string Base64jpegPrefix = "data:image/jepg;base64,";
        public const string Base64jpgPrefix = "data:image/jpg;base64,";
        public const string Base64mp4Prefix = "data:video/mp4;base64,";
        public const string TempPrefix = "temp_";
        public const string ConvertedPrefix = "processed_";
        public const string ThumbnailPrefix = "thumbnail_";

        public static string GenerateConvertedFileName(string guid) => $"{ConvertedPrefix}{guid}.mp4";
        public static string GenerateThumbnailFileName(string guid) => $"{ThumbnailPrefix}{guid}.jpg";
    }

    public struct Paths
    {
        public const string SecretSettings = @"secrets/appsettings.secrets.json";
    }

    public struct CustomEnvironment
    {
        public const string Test = nameof(Test);
    }

    public struct PushNotificationTitle
    {
        public const string NewMessage = "New Message";
        public const string NewDirectMessage = "Direct Message";
        public const string NewInvitation = "New Invitation";
        public const string InvitationApproved = "Invitation Approved";
        public const string EventReminder = "Event Reminder";
        public const string VideoCall = "Video Call";
    }

    public struct ApiResponseMessage
    {
        public const string DefaultFail = "Something went wrong";
    }

    public struct AuthorizationPolicy
    {
        public const string AdminPolicy = nameof(AdminPolicy);
    }

    public struct ClaimType
    {
        public const string Role = nameof(Role);
    }

    public struct Role
    {
        public const string Admin = nameof(Admin);
    }

    public struct CacheProfiles
    {
        public const string NoStore = nameof(NoStore);
    }

    public struct ContactState
    {
        public const string Online = nameof(Online);
        public const string Offline = nameof(Offline);
        public const string Unknown = nameof(Unknown);
    }

    public enum PushNotificationType
    {
        IncomingVideoCall,
        IncomingMessage,
        IncomingDirectMessage,
        IncomingContactRequest,
        ContactRequestApproved,
        EventReminder,
    }

    public struct ContentTypes
    {
        public const string Json = "application/json";
    }

    public struct CorsPolicy
    {
        public const string WebClient = nameof(WebClient);
    }

    public struct RateLimiting
    {
        public const string AnonymusRestricted = nameof(AnonymusRestricted);
    }

    public struct StringLength
    {
        public const int NameMax = 50;
        public const int TextMax = 3000;
        public const int Max = 100_000_000;
        public const int DataStoreMax = 5000;
        public const int PasswordMax = 20;
        public const int PasswordMin = 8;
        public const int UserNameMax = 10;
        public const int UserNameMin = 3;
    }
}
