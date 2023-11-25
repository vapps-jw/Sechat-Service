using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Queries.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.QueryHandlers;

public class GetEventQueryHandler : IRequestHandler<GetEventQuery, CalendarEventDto>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public GetEventQueryHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarEventDto> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var ce = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id.Equals(request.EventId) && e.Calendar.UserProfileId.Equals(request.UserId), cancellationToken);
        return ce is null ? null : _mapper.Map<CalendarEventDto>(ce);
    }
}
