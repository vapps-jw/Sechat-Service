namespace Sechat.Data.Models.UserDetails;
public record NotificationSubscription
{
    public int Id { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public DateTime? ExpirationTime { get; set; }
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}

