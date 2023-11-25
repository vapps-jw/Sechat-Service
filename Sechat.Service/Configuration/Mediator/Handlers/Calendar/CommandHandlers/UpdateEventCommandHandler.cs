using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, CalendarEventDto>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public UpdateEventCommandHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarEventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var storedEvent = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id.Equals(request.Id) && e.Calendar.UserProfileId.Equals(request), cancellationToken);
        if (storedEvent is null) return null;
        storedEvent.Data = request.Data;

        return await _context.SaveChangesAsync(cancellationToken) > 0 ? _mapper.Map<CalendarEventDto>(storedEvent) : null;
    }
}
