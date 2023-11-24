using FluentValidation;
using MediatR;
using Sechat.Service.Dtos.CalendarDtos;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class CreateEventCommand : IRequest<CalendarEventDto>
{
    public string UserId { get; set; }
    public string Data { get; set; }
}

public class NewEventCommandValidation : AbstractValidator<CreateEventCommand>
{
    public NewEventCommandValidation() => _ = RuleFor(x => x.Data).NotNull().NotEmpty().MaximumLength(AppConstants.StringLength.DataStoreMax);
}
