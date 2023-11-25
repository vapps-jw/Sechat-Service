using MediatR;
using System.Collections.Generic;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class DeleteEventsCommand : IRequest<int>
{
    public List<string> EventIds { get; set; }
    public string UserId { get; set; }
}
