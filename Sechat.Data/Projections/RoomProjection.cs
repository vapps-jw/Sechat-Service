using Sechat.Data.Models;

namespace Sechat.Data.Projections;
public record RoomProjection : BaseModel<long>
{
    public string CreatorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public List<string> Members { get; set; } = new();
}
