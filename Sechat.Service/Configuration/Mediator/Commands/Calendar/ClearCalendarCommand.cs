using MediatR;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class ClearCalendarCommand : IRequest<bool>
{
    public string UserId { get; }

    public ClearCalendarCommand(string userId) => UserId = userId;
}
