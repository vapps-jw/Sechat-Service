using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CalendarDtos;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        calendar.CalendarEvents.Clear();
        _ = await ctx.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    // Events

    //[HttpPost("event")]
    //public async Task<IActionResult> CreateEvent(CancellationToken cancellationToken, [FromBody] CalendarEventDto dto)
    //{
    //    using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

    //    var calendarEvent = _mapper.Map<CalendarEvent>(dto);
    //    var calendar = ctx.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
    //    calendar.CalendarEvents.Add(calendarEvent);

    //    return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    //}

    [HttpPut("event")]
    public async Task<IActionResult> UpdateEvent(CancellationToken cancellationToken, [FromBody] CalendarEventDto dto)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = _mapper.Map<CalendarEvent>(dto);
        _ = ctx.CalendarEvents.Update(ce);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    [HttpDelete("event/{eventId}")]
    public async Task<IActionResult> DeleteEventAsync(CancellationToken cancellationToken, string eventId)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var ce = ctx.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId));
        if (ce is null) return BadRequest();
        _ = ctx.CalendarEvents.Remove(ce);

        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Ok() : BadRequest();
    }

    // Reminders

    [HttpPost("event/{eventId}/reminder")]
    public IActionResult AddReminder([FromBody] ReminderDto reminder, string eventId) => Ok();

    [HttpDelete("event/{eventId}/{reminderId}")]
    public IActionResult DeleteReminder(string eventId, long reminderId) => Ok();
}

public class CalendarControllerForms
{
    public class CreateCalendarForm
    {
        public string Name { get; set; }
    }
    public class CreateCalendarFormValidation : AbstractValidator<CreateCalendarForm>
    {
        public CreateCalendarFormValidation() => _ = RuleFor(x => x.Name).NotEmpty().MaximumLength(AppConstants.StringLengths.NameMax);
    }

    public class UpdateCalendarForm
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class UpdateCalendarFormValidation : AbstractValidator<UpdateCalendarForm>
    {
        public UpdateCalendarFormValidation()
        {
            _ = RuleFor(x => x.Id).NotEmpty();
            _ = RuleFor(x => x.Name).NotEmpty().MaximumLength(AppConstants.StringLengths.NameMax);
        }
    }

    public class CalendarEventDtoValidation : AbstractValidator<CalendarEventDto>
    {
        public CalendarEventDtoValidation()
        {
            _ = RuleFor(x => x.Name).NotEmpty().MaximumLength(AppConstants.StringLengths.NameMax);
            _ = RuleFor(x => x.Description).NotEmpty().MaximumLength(AppConstants.StringLengths.TextMax);
            _ = RuleFor(x => x.IsAllDay).NotNull();
            _ = RuleFor(x => x.Start).NotNull();
            _ = RuleFor(x => x.End).NotNull();
        }
    }
}
