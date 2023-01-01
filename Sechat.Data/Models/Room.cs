namespace Sechat.Data.Models;

public record Room : BaseModel<long>
{
    public string RoomKey { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public List<Message> Messages { get; set; } = new();
}
