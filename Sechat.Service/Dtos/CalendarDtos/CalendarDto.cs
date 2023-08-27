using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.CalendarDtos;

public class CalendarDto
{
    public List<CalendarEventDto> CalendarEvents { get; set; } = new();
}

public class ReminderDto
{
    public long Id { get; set; }
    public DateTime Remind { get; set; }

    public string CalendarEventId { get; set; }
}

public class CalendarEventDto
{
    public string Id { get; set; }

    public string Data { get; set; }

    public List<ReminderDto> Reminders { get; set; }
}
