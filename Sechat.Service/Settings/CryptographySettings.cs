namespace Sechat.Service.Settings;

public class CryptographySettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
}
