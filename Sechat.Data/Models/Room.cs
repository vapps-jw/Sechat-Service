namespace Sechat.Data.Models;

public record Room : BaseModel<long>
{
    public string CreatorId { get; set; } = string.Empty;

    public string RoomKey { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public List<Message> Messages { get; set; } = new();
    public List<UserProfile> Members { get; set; } = new();
}
