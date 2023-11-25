using MediatR;
using Sechat.Service.Dtos.CalendarDtos;
using System.Collections.Generic;
using static Sechat.Service.Controllers.CalendarControllerForms;

namespace Sechat.Service.Configuration.Mediator.Commands.Calendar;

public class CreateRemindersCommand : IRequest<List<ReminderDto>>
{
    public CreateRemindersCommand(string eventId, string userId, List<NewReminderForm> createReminderCommands)
    {
        EventId = eventId;
        UserId = userId;
        CreateReminderCommands = createReminderCommands;
    }

    public string EventId { get; set; }
    public string UserId { get; set; }
    public List<NewReminderForm> CreateReminderCommands { get; set; } = new();

}
