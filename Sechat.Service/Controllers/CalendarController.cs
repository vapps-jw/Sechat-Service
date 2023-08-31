using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CalendarDtos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Sechat.Service.Controllers.CalendarControllerForms;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class CalendarController : SechatControllerBase
{
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly IMapper _mapper;

    public CalendarController(
        IDbContextFactory<SechatContext> contextFactory,
        IMapper mapper)
    {
        _contextFactory = contextFactory;
        _mapper = mapper;
    }

    // Calendar

    [HttpGet()]
    public async Task<IActionResult> GetCalendar(CancellationToken cancellationToken)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var calendar = ctx.Calendars
            .Where(c => c.UserProfileId.Equals(UserId))
            .AsSplitQuery()
            .Include(c => c.CalendarEvents)
            .ThenInclude(ce => ce.Reminders)
            .FirstOrDefault();
        if (calendar is null) return BadRequest();

        var dto = _mapper.Map<CalendarDto>(calendar);
        return Ok(dto);
    }

    [HttpDelete()]
    public async Task<IActionResult> ClearCalendar(CancellationToken cancellationToken)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var calendar = ctx.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
        if (calendar is null) return BadRequest();

        ctx.CalendarEvents.RemoveRange(ctx.CalendarEvents.Where(ce => ce.CalendarId.Equals(calendar.Id)));
        _ = await ctx.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    // Events

    [HttpGet("event/{eventId}", Name = nameof(GetEvent))]
    public async Task<IActionResult> GetEvent(CancellationToken cancellationToken, string eventId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId) && e.Calendar.UserProfileId.Equals(UserId));
        if (ce is null) return BadRequest();
        _ = ctx.CalendarEvents.Remove(ce);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    [HttpPost("event")]
    public async Task<IActionResult> CreateEvent(CancellationToken cancellationToken, [FromBody] NewEventForm form)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var calendar = ctx.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
        var newEvent = new CalendarEvent()
        {
            Id = Guid.NewGuid().ToString(),
            Data = form.Data,
        };

        calendar.CalendarEvents.Add(newEvent);

        var response = CreatedAtRoute(nameof(GetEvent), new { eventId = newEvent.Id }, _mapper.Map<CalendarEventDto>(newEvent));
        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? response : BadRequest();
    }

    [HttpPut("event")]
    public async Task<IActionResult> UpdateEvent(CancellationToken cancellationToken, [FromBody] CalendarEventDto dto)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var existingEvent = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(dto.Id) && e.Calendar.UserProfileId.Equals(UserId));
        if (existingEvent is null) return BadRequest();
        existingEvent.Data = dto.Data;
        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    [HttpDelete("event")]
    public async Task<IActionResult> DeleteEventAsync(CancellationToken cancellationToken, string eventId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId) && e.Calendar.UserProfileId.Equals(UserId));
        if (ce is null) return BadRequest();
        _ = ctx.CalendarEvents.Remove(ce);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    // Reminders

    [HttpPost("event/reminder")]
    public async Task<IActionResult> CreateReminder(CancellationToken cancellationToken, [FromBody] NewReminderForm reminder)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(reminder.EventId) && e.Calendar.UserProfileId.Equals(UserId));
        if (ce is null) return BadRequest();

        var newReminder = new Reminder() { Remind = reminder.Remind.ToUniversalTime() };
        ce.Reminders.Add(newReminder);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok(_mapper.Map<ReminderDto>(newReminder)) : BadRequest();
    }

    [HttpDelete("event/{eventId}/{reminderId}")]
    public async Task<IActionResult> DeleteReminder(CancellationToken cancellationToken, string eventId, long reminderId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents
            .Where(e => e.Id.Equals(eventId) && e.Calendar.UserProfileId.Equals(UserId))
            .Include(e => e.Reminders)
            .FirstOrDefault();
        if (ce is null) return BadRequest();

        var reminderToDelete = ce.Reminders.FirstOrDefault(r => r.Id == reminderId);
        if (reminderToDelete is null) return BadRequest();

        _ = ctx.Reminders.Remove(reminderToDelete);
        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }
}

public class CalendarControllerForms
{
    public class NewEventForm
    {
        public string Data { get; set; }
    }
    public class NewEventFormValidation : AbstractValidator<NewEventForm>
    {
        public NewEventFormValidation() => _ = RuleFor(x => x.Data).NotNull().NotEmpty().MaximumLength(AppConstants.StringLengths.DataStoreMax);
    }

    public class NewReminderForm
    {
        public string EventId { get; set; }
        public DateTime Remind { get; set; }
    }
    public class NewReminderFormValidation : AbstractValidator<NewReminderForm>
    {
        public NewReminderFormValidation()
        {
            _ = RuleFor(x => x.Remind).NotNull().NotEmpty();
            _ = RuleFor(x => x.EventId).NotNull().NotEmpty();
        }
    }
}
