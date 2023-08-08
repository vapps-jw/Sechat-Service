using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using static Sechat.Service.Controllers.CalendarController.CalendarControllerForms;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class CalendarController : SechatControllerBase
{
    private readonly CalendarRepository _calendarRepository;

    public CalendarController(CalendarRepository calendarRepository) => _calendarRepository = calendarRepository;

    // Calendar

    [HttpGet("{calendarId}")]
    public IActionResult GetCalendar(string calendarId) => Ok();

    [HttpDelete("{calendarId}")]
    public IActionResult DeleteCalendar(string calendarId) => Ok();

    [HttpPatch("{calendarId}")]
    public IActionResult UpdateCalendar(string calendarId) => Ok();

    [HttpPost()]
    public IActionResult CreateCalendar([FromBody] CreateCalendarForm createCalendarForm) => Ok();

    // Events

    [HttpPost("calendar-event")]
    public IActionResult CreateEvent() => Ok();

    [HttpPatch("calendar-event")]
    public IActionResult UpdateEvent() => Ok();

    [HttpDelete("calendar-event")]
    public IActionResult DeleteEvent() => Ok();

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
    }
}
