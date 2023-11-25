using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class DeleteRemindersCommandHandler : IRequestHandler<DeleteRemindersCommand, int>
{
    private readonly SechatContext _context;

    public DeleteRemindersCommandHandler(SechatContext context) => _context = context;

    public async Task<int> Handle(DeleteRemindersCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.Reminders
            .Where(r => r.CalendarEventId.Equals(request.EventId))
            .ExecuteDeleteAsync(cancellationToken);
        return result;
    }
}

