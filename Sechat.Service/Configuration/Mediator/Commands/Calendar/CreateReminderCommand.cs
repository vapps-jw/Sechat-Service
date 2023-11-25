using FluentValidation;
using MediatR;
using Sechat.Service.Dtos.CalendarDtos;
using System;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class CreateReminderCommand : IRequest<ReminderDto>
{
    public string UserId { get; set; }
    public string EventId { get; set; }
    public DateTime Remind { get; set; }
}
public class CreateReminderCommandValidation : AbstractValidator<CreateReminderCommand>
{
    public CreateReminderCommandValidation()
    {
        _ = RuleFor(x => x.Remind).NotNull().NotEmpty();
        _ = RuleFor(x => x.EventId).NotNull().NotEmpty();
    }
}
