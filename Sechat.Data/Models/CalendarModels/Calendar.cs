using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.CalendarModels;
public record Calendar : BaseModel<string>
{
    public List<CalendarEvent> CalendarEvents { get; set; }
    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
