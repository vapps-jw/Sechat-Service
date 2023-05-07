using System.Collections.Generic;

namespace Sechat.Service.Services;

public class SignalRConnectionsMonitor
{
    public List<string> ConnectedUsers { get; set; } = new();
}
