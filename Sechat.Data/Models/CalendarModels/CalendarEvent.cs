using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.CalendarModels;
public record CalendarEvent : BaseModel<long>
{
    public string Name { get; set; }
    public string Description { get; set; }

    public bool AllDay { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public long CalendarId { get; set; }
    public Calendar Calendar { get; set; }
}
