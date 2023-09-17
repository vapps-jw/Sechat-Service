namespace Sechat.Service.Configuration;

public class AppGlobalSettings
{
    public struct SettingName
    {
        public const string RegistrationStatus = nameof(RegistrationStatus);
    }

    public struct RegistrationStatus
    {
        public const string Allowed = nameof(Allowed);
        public const string Forbidden = nameof(Forbidden);
    }
}
