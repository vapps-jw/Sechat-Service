using Sechat.Service.Configuration;
using System.Collections.Generic;

namespace Sechat.Service.Services;

public class SignalRConnectionsMonitor
{
    public List<string> ConnectedUsers { get; set; } = new();

    public string IsUserOnline(string userId) =>
        ConnectedUsers.Contains(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
}
