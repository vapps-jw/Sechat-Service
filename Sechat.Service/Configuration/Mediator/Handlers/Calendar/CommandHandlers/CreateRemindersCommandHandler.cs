using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Dtos.CalendarDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class CreateRemindersCommandHandler : IRequestHandler<CreateRemindersCommand, List<ReminderDto>>
{
    private readonly SechatContext _context;
    private readonly IMapper _mapper;

    public CreateRemindersCommandHandler(SechatContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReminderDto>> Handle(CreateRemindersCommand request, CancellationToken cancellationToken)
    {
        var ce = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id.Equals(request.EventId) && e.Calendar.UserProfileId.Equals(request.UserId), cancellationToken);
        if (ce is null) return null;

        var newReminders = request.CreateReminderCommands.Select(r => new Reminder() { Remind = r.Remind.ToUniversalTime() }).ToList();
        ce.Reminders.AddRange(newReminders);

        return await _context.SaveChangesAsync(cancellationToken) > 0 ? _mapper.Map<List<ReminderDto>>(newReminders) : null;
    }
}
