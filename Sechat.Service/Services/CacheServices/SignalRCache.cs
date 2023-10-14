using Sechat.Service.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services.CacheServices;

public class SignalRCache
{
    private readonly SemaphoreSlim _signalRSemaphore;

    public Dictionary<string, HashSet<string>> ConnectedUsers { get; set; } = new();

    public SignalRCache() => _signalRSemaphore = new SemaphoreSlim(1);

    public string IsUserOnline(string userId) =>
        ConnectedUsers.ContainsKey(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsUserOnlineFlag(string userId) => ConnectedUsers.ContainsKey(userId);

    public async Task AddUser(string userId, string connectionId)
    {
        await _signalRSemaphore.WaitAsync();
        if (ConnectedUsers.ContainsKey(userId))
        {
            _ = ConnectedUsers[userId].Add(connectionId);
        }
        ConnectedUsers.Add(userId, new HashSet<string>() { connectionId });
        _ = _signalRSemaphore.Release();
    }

    public async Task RemoveUserConnection(string userId, string connectionId)
    {
        await _signalRSemaphore.WaitAsync();
        if (ConnectedUsers.ContainsKey(userId))
        {
            _ = ConnectedUsers[userId].Remove(connectionId);
        }
        _ = _signalRSemaphore.Release();
    }
}
