using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand, int>
{
    private readonly SechatContext _context;

    public DeleteReminderCommandHandler(SechatContext context) => _context = context;

    public async Task<int> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var ce = await _context.CalendarEvents
            .Where(e => e.Id.Equals(request.EventId) && e.Calendar.UserProfileId.Equals(request.UserId))
            .Include(e => e.Reminders)
                .FirstOrDefaultAsync(cancellationToken);
        if (ce is null) return -1;

        var reminderToDelete = ce.Reminders.FirstOrDefault(r => r.Id == request.ReminderId);
        if (reminderToDelete is null) return -1;

        _ = _context.Reminders.Remove(reminderToDelete);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
