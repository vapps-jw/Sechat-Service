using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.CalendarModels;
public record Calendar : BaseModel<string>
{
    public string Name { get; set; }
    public string Color { get; set; }

    public List<CalendarEvent> CalendarEvents { get; set; }
    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
