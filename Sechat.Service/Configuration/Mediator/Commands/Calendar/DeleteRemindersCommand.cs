using MediatR;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class DeleteRemindersCommand : IRequest<int>
{
    public DeleteRemindersCommand(string userId, string eventId)
    {
        UserId = userId;
        EventId = eventId;
    }

    public string UserId { get; set; }
    public string EventId { get; set; }
}
