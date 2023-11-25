using FluentValidation;
using MediatR;
using Sechat.Service.Dtos.CalendarDtos;
using System.Collections.Generic;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class UpdateEventCommand : IRequest<CalendarEventDto>
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Data { get; set; }

    public List<ReminderDto> Reminders { get; set; }
}

public class UpdateEventCommandValidation : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidation()
    {
        _ = RuleFor(x => x.Id).NotNull().NotEmpty();
        _ = RuleFor(x => x.Data).NotNull().NotEmpty().MaximumLength(AppConstants.StringLength.DataStoreMax);
    }
}

