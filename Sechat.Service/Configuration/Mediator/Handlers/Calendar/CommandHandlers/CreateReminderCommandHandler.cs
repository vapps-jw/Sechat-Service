using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class CreateReminderCommandHandler : IRequestHandler<CreateReminderCommand, ReminderDto>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public CreateReminderCommandHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReminderDto> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var ce = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id.Equals(request.EventId) && e.Calendar.UserProfileId.Equals(request.UserId), cancellationToken);
        if (ce is null) return null;

        var newReminder = new Reminder() { Remind = request.Remind.ToUniversalTime() };
        ce.Reminders.Add(newReminder);

        return await _context.SaveChangesAsync(cancellationToken) > 0 ? _mapper.Map<ReminderDto>(newReminder) : null;
    }
}
