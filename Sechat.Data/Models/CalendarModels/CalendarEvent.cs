using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.CalendarModels;
public record CalendarEvent : BaseModel<string>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }

    public string IsAllDay { get; set; }
    public string Day { get; set; }

    public string Start { get; set; }
    public string End { get; set; }

    public string CalendarId { get; set; }
    public Calendar Calendar { get; set; }

    public List<Reminder> Reminders { get; set; }
}
