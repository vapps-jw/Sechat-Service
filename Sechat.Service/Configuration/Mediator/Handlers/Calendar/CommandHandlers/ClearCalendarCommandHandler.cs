using MediatR;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class ClearCalendarCommandHandler : IRequestHandler<ClearCalendarCommand, bool>
{
    private readonly SechatContext _context;

    public ClearCalendarCommandHandler(SechatContext context) => _context = context;

    public async Task<bool> Handle(ClearCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = _context.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(request.UserId));
        if (calendar is null) return false;

        _context.CalendarEvents.RemoveRange(_context.CalendarEvents.Where(ce => ce.CalendarId.Equals(calendar.Id)));

        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
