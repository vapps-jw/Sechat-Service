﻿using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.CalendarDtos;

public class CalendarDto
{
    public string Id { get; set; }
    public string Name { get; set; }

    public List<CalendarEventDto> CalendarEventDtos { get; set; }
}

public class CalendarEventDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public bool IsAllDay { get; set; }
    public DateOnly AllDay { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}