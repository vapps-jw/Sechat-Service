using Sechat.Service.Configuration;
using System.Collections.Generic;

namespace Sechat.Service.Services;

public class SignalRConnectionsMonitor
{
    public List<string> ConnectedUsers { get; set; } = new();

    public string IsUserOnline(string userId) =>
        ConnectedUsers.Contains(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsUserOnlineFlag(string userId) => ConnectedUsers.Contains(userId);

    public void AddUser(string userId) => ConnectedUsers.Add(userId);

    public void RemoveAllUserConnections(string userId) => ConnectedUsers.RemoveAll(u => u.Equals(userId));

    public void RemoveUserConnection(string userId) => ConnectedUsers.Remove(userId);
}
