namespace Sechat.Data.Models;
public record Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PushEndpoint { get; set; } = string.Empty;
    public string PushP256DH { get; set; } = string.Empty;
    public string PushAuth { get; set; } = string.Empty;

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
