using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Configuration.Mediator.Queries.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System;
using System.Collections.Generic;
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
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly IMapper _mapper;

    public CalendarController(
        IMediator mediator,
        IDbContextFactory<SechatContext> contextFactory,
        IMapper mapper)
    {
        _mediator = mediator;
        _contextFactory = contextFactory;
        _mapper = mapper;
    }

    // Calendar

    [HttpGet()]
    public async Task<IActionResult> GetCalendarAsync(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCalendarQuery(UserId), cancellationToken);
        return result is null ? BadRequest() : Ok(result);
    }

    [HttpDelete()]
    public async Task<IActionResult> ClearCalendar(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ClearCalendarCommand(UserId), cancellationToken);
        return result ? Ok() : BadRequest();
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
    public async Task<IActionResult> CreateEvent(CancellationToken cancellationToken, [FromBody] CreateEventCommand command)
    {
        command.UserId = UserId;
        var result = await _mediator.Send(command);

        return result is null
            ? BadRequest()
            : CreatedAtRoute(nameof(GetEvent), new { eventId = result.Id }, _mapper.Map<CalendarEventDto>(result));
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
    public async Task<IActionResult> DeleteEvent(CancellationToken cancellationToken, string eventId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId) && e.Calendar.UserProfileId.Equals(UserId));
        if (ce is null) return BadRequest();
        _ = ctx.CalendarEvents.Remove(ce);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    [HttpPost("delete-events")]
    public async Task<IActionResult> DeleteEvents(CancellationToken cancellationToken, List<string> ids)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await ctx.CalendarEvents.Where(e => ids.Contains(e.Id) && e.Calendar.UserProfileId.Equals(UserId)).ExecuteDeleteAsync();

        return result == 0 ? BadRequest() : Ok();
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

    [HttpPost("event/{eventId}/reminders")]
    public async Task<IActionResult> CreateReminders(CancellationToken cancellationToken, string eventId, [FromBody] List<NewReminderForm> reminders)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId) && e.Calendar.UserProfileId.Equals(UserId));
        if (ce is null) return BadRequest();

        var newReminders = reminders.Select(r => new Reminder() { Remind = r.Remind.ToUniversalTime() }).ToList();
        ce.Reminders.AddRange(newReminders);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok(_mapper.Map<List<ReminderDto>>(newReminders)) : BadRequest();
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

    [HttpDelete("event/{eventId}/reminders")]
    public async Task<IActionResult> DeleteReminders(CancellationToken cancellationToken, string eventId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var result = ctx.Reminders
            .Where(r => r.CalendarEventId.Equals(eventId))
            .ExecuteDelete();
        return result == 0 ? BadRequest() : Ok();
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
        public NewEventFormValidation() => _ = RuleFor(x => x.Data).NotNull().NotEmpty().MaximumLength(AppConstants.StringLength.DataStoreMax);
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
