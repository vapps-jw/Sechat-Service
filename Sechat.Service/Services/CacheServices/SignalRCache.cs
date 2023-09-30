using Sechat.Service.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services.CacheServices;

public class SignalRCache
{
    private readonly SemaphoreSlim _signalRSemaphore;

    public List<string> ConnectedUsers { get; set; } = new();

    public SignalRCache() => _signalRSemaphore = new SemaphoreSlim(0, 1);

    public string IsUserOnline(string userId) =>
        ConnectedUsers.Contains(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsUserOnlineFlag(string userId) => ConnectedUsers.Contains(userId);

    public async Task AddUser(string userId)
    {
        await _signalRSemaphore.WaitAsync();
        ConnectedUsers.Add(userId);
        _ = _signalRSemaphore.Release();
    }

    public async Task RemoveAllUserConnections(string userId)
    {
        await _signalRSemaphore.WaitAsync();
        _ = ConnectedUsers.RemoveAll(u => u.Equals(userId));
        _ = _signalRSemaphore.Release();
    }

    public async Task RemoveUserConnection(string userId)
    {
        await _signalRSemaphore.WaitAsync();
        _ = ConnectedUsers.Remove(userId);
        _ = _signalRSemaphore.Release();
    }
}
