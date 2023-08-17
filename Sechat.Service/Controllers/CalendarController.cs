using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CalendarDtos;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class CalendarController : SechatControllerBase
{
    private readonly CalendarRepository _calendarRepository;

    public CalendarController(CalendarRepository calendarRepository) => _calendarRepository = calendarRepository;

    // Calendar

    [HttpGet("{calendarId}")]
    [ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoCache)]
    public IActionResult GetCalendar(string calendarId) => Ok();

    [HttpDelete("{calendarId}")]
    public IActionResult DeleteCalendar(string calendarId) => Ok();

    [HttpPatch("{calendarId}")]
    public IActionResult UpdateCalendar([FromBody] CalendarControllerForms.CreateCalendarForm form, string calendarId) => Ok();

    [HttpPost()]
    public IActionResult CreateCalendar([FromBody] CalendarControllerForms.CreateCalendarForm form) => Ok();

    // Events

    [HttpPost("calendar-event")]
    public IActionResult CreateEvent([FromBody] CalendarEventDto dto) => Ok();

    [HttpPatch("calendar-event")]
    public IActionResult UpdateEvent([FromBody] CalendarEventDto dto) => Ok();

    [HttpDelete("calendar-event/{eventId}")]
    public IActionResult DeleteEvent(string eventId) => Ok();

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
