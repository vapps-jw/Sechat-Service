using MediatR;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class DeleteReminderCommand : IRequest<int>
{
    public DeleteReminderCommand(string userId, string eventId, long reminderId)
    {
        UserId = userId;
        EventId = eventId;
        ReminderId = reminderId;
    }

    public string UserId { get; set; }
    public string EventId { get; set; }
    public long ReminderId { get; set; }
}
