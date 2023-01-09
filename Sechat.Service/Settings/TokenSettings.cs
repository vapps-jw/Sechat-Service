namespace Sechat.Service.Settings;

public class TokenSettings
{
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
}
