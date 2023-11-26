using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Queries.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.QueryHandlers;

public class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, CalendarDto>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public GetCalendarQueryHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarDto> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var calendar = await _context.Calendars
            .Where(c => c.UserProfileId.Equals(request.UserId))
            .AsSplitQuery()
            .Include(c => c.CalendarEvents)
            .ThenInclude(ce => ce.Reminders)
            .FirstOrDefaultAsync(cancellationToken);

        return calendar is null ? null : _mapper.Map<CalendarDto>(calendar);
    }
}
