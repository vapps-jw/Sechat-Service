using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Models.CalendarModels;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CalendarDtos;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class CalendarController : SechatControllerBase
{
    private readonly IMapper _mapper;
    private readonly CalendarRepository _calendarRepository;

    public CalendarController(
        IMapper mapper,
        CalendarRepository calendarRepository)
    {
        _mapper = mapper;
        _calendarRepository = calendarRepository;
    }

    // Calendar

    [HttpGet()]
    public IActionResult GetCalendar()
    {
        var calendar = _calendarRepository.GetCalendar(UserId);
        if (calendar is null) return BadRequest();

        var dto = _mapper.Map<CalendarDto>(calendar);
        return Ok(dto);
    }

    [HttpDelete()]
    public async Task<IActionResult> ClearCalendar()
    {
        var calendar = _calendarRepository.GetCalendar(UserId);
        if (calendar is null) return BadRequest();
        calendar.CalendarEvents.Clear();
        _ = await _calendarRepository.SaveChanges();

        return Ok();
    }

    // Events

    [HttpPost("event")]
    public async Task<IActionResult> CreateEvent([FromBody] CalendarEventDto dto)
    {
        var calendarEvent = _mapper.Map<CalendarEvent>(dto);
        _calendarRepository.AddEvent(UserId, calendarEvent);
        return await _calendarRepository.SaveChanges() > 0 ? Ok() : BadRequest();
    }

    [HttpPatch("event")]
    public IActionResult UpdateEvent([FromBody] CalendarEventDto dto) => Ok();

    [HttpDelete("event/{eventId}")]
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
