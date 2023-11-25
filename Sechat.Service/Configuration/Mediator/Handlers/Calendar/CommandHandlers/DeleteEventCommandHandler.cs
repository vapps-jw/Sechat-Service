using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, int>
{
    private readonly SechatContext _context;

    public DeleteEventCommandHandler(SechatContext context) => _context = context;

    public async Task<int> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var ce = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id.Equals(request.EventId) && e.Calendar.UserProfileId.Equals(request.UserId), cancellationToken);
        if (ce is null) return -1;
        _ = _context.CalendarEvents.Remove(ce);

        return await _context.SaveChangesAsync(cancellationToken);
    }
}
