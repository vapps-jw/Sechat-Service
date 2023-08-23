using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CalendarDtos;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class CalendarController : SechatControllerBase
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public CalendarController(
        SechatContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Calendar

    [HttpGet()]
    public IActionResult GetCalendar()
    {
        var calendar = _context.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
        if (calendar is null) return BadRequest();

        var dto = _mapper.Map<CalendarDto>(calendar);
        return Ok(dto);
    }

    [HttpDelete()]
    public async Task<IActionResult> ClearCalendar()
    {
        var calendar = _context.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
        if (calendar is null) return BadRequest();

        calendar.CalendarEvents.Clear();
        _ = await _context.SaveChangesAsync();

        return Ok();
    }

    // Events

    [HttpPost("event")]
    public async Task<IActionResult> CreateEvent([FromBody] CalendarEventDto dto)
    {
        var calendarEvent = _mapper.Map<CalendarEvent>(dto);
        var calendar = _context.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(UserId));
        calendar.CalendarEvents.Add(calendarEvent);
        return await _context.SaveChangesAsync() > 0 ? Ok() : BadRequest();
    }

    [HttpPut("event")]
    public async Task<IActionResult> UpdateEvent([FromBody] CalendarEventDto dto)
    {
        var ce = _mapper.Map<CalendarEvent>(dto);
        _ = _context.CalendarEvents.Update(ce);

        return await _context.SaveChangesAsync() > 0 ? Ok() : BadRequest();
    }

    [HttpDelete("event/{eventId}")]
    public async Task<IActionResult> DeleteEventAsync(string eventId)
    {
        var ce = _context.CalendarEvents.FirstOrDefault(e => e.Id.Equals(eventId));
        if (ce is null) return BadRequest();

        _ = _context.CalendarEvents.Remove(ce);
        return await _context.SaveChangesAsync() > 0 ? Ok() : BadRequest();
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
