using MediatR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Service.Configuration.Mediator.Commands.Calendar;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Configuration.Mediator.Handlers.Calendar.CommandHandlers;

public class DeleteEventsCommandHandler : IRequestHandler<DeleteEventsCommand, int>
{
    private readonly SechatContext _context;

    public DeleteEventsCommandHandler(SechatContext context) => _context = context;

    public Task<int> Handle(DeleteEventsCommand request, CancellationToken cancellationToken) =>
        _context.CalendarEvents.Where(e => request.EventIds.Contains(e.Id) && e.Calendar.UserProfileId.Equals(request.UserId)).ExecuteDeleteAsync(cancellationToken);
}
