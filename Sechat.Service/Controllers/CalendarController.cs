using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Configuration.Mediator.Queries.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System;
using System.Collections.Generic;
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
        return result.Success ? Ok() : BadRequest(result.ErrorMessage);
    }

    // Events

    [HttpGet("event/{eventId}", Name = nameof(GetEvent))]
    public async Task<IActionResult> GetEvent(string eventId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEventQuery(eventId, UserId), cancellationToken);
        return result is null ? BadRequest() : Ok(result);
    }

    [HttpPost("event")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await _mediator.Send(command);

        return result is null
            ? BadRequest()
            : CreatedAtRoute(nameof(GetEvent), new { eventId = result.Id }, _mapper.Map<CalendarEventDto>(result));
    }

    [HttpPut("event")]
    public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventCommand command, CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await _mediator.Send(command, cancellationToken);

        return result is null
            ? BadRequest()
            : Ok(result);
    }

    [HttpDelete("event")]
    public async Task<IActionResult> DeleteEvent(string eventId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteEventCommand() { EventId = eventId, UserId = UserId }, cancellationToken);
        return result > 0
            ? Ok()
            : BadRequest();
    }

    [HttpPost("delete-events")]
    public async Task<IActionResult> DeleteEvents(List<string> ids, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteEventsCommand() { EventIds = ids, UserId = UserId }, cancellationToken);
        return result > 0
            ? Ok()
            : BadRequest();
    }

    // Reminders

    [HttpPost("event/reminder")]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderCommand reminder, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateReminderCommand() { EventId = reminder.EventId, Remind = reminder.Remind, UserId = UserId }, cancellationToken);
        return result is not null
            ? Ok(result)
            : BadRequest();
    }

    [HttpPost("event/{eventId}/reminders")]
    public async Task<IActionResult> CreateReminders(string eventId, [FromBody] List<NewReminderForm> reminders, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRemindersCommand(eventId, UserId, reminders), cancellationToken);
        return result is not null
            ? Ok(result)
            : BadRequest();
    }

    [HttpDelete("event/{eventId}/{reminderId}")]
    public async Task<IActionResult> DeleteReminder(string eventId, long reminderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteReminderCommand(UserId, eventId, reminderId), cancellationToken);
        return result > 0
            ? Ok()
            : BadRequest();
    }

    [HttpDelete("event/{eventId}/reminders")]
    public async Task<IActionResult> DeleteRemindersAsync(string eventId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRemindersCommand(UserId, eventId), cancellationToken);
        return result > 0
            ? Ok()
            : BadRequest();
    }
}

public class CalendarControllerForms
{
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
