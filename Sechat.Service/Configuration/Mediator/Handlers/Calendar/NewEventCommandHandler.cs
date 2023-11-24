using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar;

public class NewEventCommandHandler : IRequestHandler<NewEventCommand, CalendarEventDto>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public NewEventCommandHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CalendarEventDto> Handle(NewEventCommand request, CancellationToken cancellationToken)
    {
        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.UserProfileId.Equals(request.UserId), cancellationToken);
        var newEvent = new CalendarEvent()
        {
            Id = Guid.NewGuid().ToString(),
            Data = request.Data,
        };

        calendar.CalendarEvents.Add(newEvent);

        return await _context.SaveChangesAsync(cancellationToken) > 0 ? _mapper.Map<CalendarEventDto>(newEvent) : null;
    }
}
