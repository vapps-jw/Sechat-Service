namespace Sechat.Service.Utilities;

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
