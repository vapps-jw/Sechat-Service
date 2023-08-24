using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.CalendarModels;
public record Reminder : BaseModel<long>
{
    public DateTime Remind { get; set; }
    public int Reminders { get; set; }
    public int Reminded { get; set; }

    public string CalendarEventId { get; set; }
    public CalendarEvent CalendarEvent { get; set; }
}
