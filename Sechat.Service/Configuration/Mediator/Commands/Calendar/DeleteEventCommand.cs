using MediatR;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class DeleteEventCommand : IRequest<int>
{
    public string EventId { get; set; }
    public string UserId { get; set; }
}
