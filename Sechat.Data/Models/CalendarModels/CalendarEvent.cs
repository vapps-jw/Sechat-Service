using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.CalendarModels;
public record CalendarEvent : BaseModel<string>
{
    public string Name { get; set; }
    public string Description { get; set; }

    public bool IsAllDay { get; set; }
    public DateOnly AllDay { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string CalendarId { get; set; }
    public Calendar Calendar { get; set; }
}
