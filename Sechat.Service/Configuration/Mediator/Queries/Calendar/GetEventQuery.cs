using MediatR;
using Sechat.Service.Dtos.CalendarDtos;

namespace Sechat.Service.Configuration.Mediator.Queries.Calendar;

public class GetEventQuery : IRequest<CalendarEventDto>
{
    public string EventId { get; }
    public string UserId { get; }

    public GetEventQuery(string eventId, string userId)
    {
        EventId = eventId;
        UserId = userId;
    }
}
