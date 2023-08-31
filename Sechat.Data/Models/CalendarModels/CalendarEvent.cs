using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.CalendarModels;
public record CalendarEvent : BaseModel<string>
{
    public string Data { get; set; }

    public string CalendarId { get; set; }
    public Calendar Calendar { get; set; }

    public List<Reminder> Reminders { get; set; } = new();
}
