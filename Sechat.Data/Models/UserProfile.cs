namespace Sechat.Data.Models;

public record UserProfile : BaseModel<string>
{
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    public List<Feature> Features { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
}
