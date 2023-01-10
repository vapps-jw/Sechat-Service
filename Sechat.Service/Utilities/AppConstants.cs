﻿namespace Sechat.Service.Utilities;

public class AppConstants
{
    public struct Paths
    {
        public const string SecretSettings = @"secrets/appsettings.secrets.json";
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
        public const string DefaultWindowPolicyName = nameof(DefaultWindowPolicyName);
    }
}