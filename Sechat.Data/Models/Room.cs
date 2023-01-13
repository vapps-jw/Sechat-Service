namespace Sechat.Data.Models;

public record Room : BaseTrackedModel<string>
{
    public string CreatorId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string RoomKey { get; set; } = string.Empty;

    public List<Message> Messages { get; set; } = new();
    public List<UserProfile> Members { get; set; } = new();
}
