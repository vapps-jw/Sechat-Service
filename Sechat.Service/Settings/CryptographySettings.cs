namespace Sechat.Service.Settings;

public class CryptographySettings
{
    public string DefaultSalt { get; set; } = string.Empty;
    public string DefaultKeyPart { get; set; } = string.Empty;
    public int DefaultInterations { get; set; }
    public string DefaultIV { get; set; }
}