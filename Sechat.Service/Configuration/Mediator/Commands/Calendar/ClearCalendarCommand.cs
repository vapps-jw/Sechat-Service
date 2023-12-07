using MediatR;
using Sechat.Service.Configuration.Mediator.Responses;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class ClearCalendarCommand : IRequest<MediatorResult<int>>
{
    public string UserId { get; }

    public ClearCalendarCommand(string userId) => UserId = userId;
}
