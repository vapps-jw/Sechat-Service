using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using Sechat.Service.Configuration.Mediator.Responses;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class ClearCalendarCommandHandler : IRequestHandler<ClearCalendarCommand, MediatorResult<int>>
{
    private readonly SechatContext _context;

    public ClearCalendarCommandHandler(SechatContext context) => _context = context;

    public async Task<MediatorResult<int>> Handle(ClearCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.UserProfileId.Equals(request.UserId), cancellationToken);
        if (calendar is null) return new MediatorResult<int>(-1, false, "Something went wrong");

        _context.CalendarEvents.RemoveRange(_context.CalendarEvents.Where(ce => ce.CalendarId.Equals(calendar.Id)));

        var res = await _context.SaveChangesAsync(cancellationToken);
        return res > 0 ? new MediatorResult<int>(res, true) : new MediatorResult<int>(res, false, "Something went wrong");
    }
}
