using MediatR;
using Sechat.Service.Dtos.CalendarDtos;

namespace Sechat.Service.Configuration.Mediator.Queries.Calendar;

public class GetCalendarQuery : IRequest<CalendarDto>
{
    public string UserId { get; }

    public GetCalendarQuery(string userId) => UserId = userId;

}
