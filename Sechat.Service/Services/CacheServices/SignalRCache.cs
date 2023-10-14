using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services.CacheServices;

public class SignalRCache
{
    private readonly SemaphoreSlim _signalRSemaphore;
    private readonly ILogger<SignalRCache> _logger;

    public Dictionary<string, HashSet<string>> ConnectedUsers { get; set; } = new();

    public SignalRCache(ILogger<SignalRCache> logger)
    {
        _signalRSemaphore = new SemaphoreSlim(1);
        _logger = logger;
    }

    public string IsUserOnline(string userId) =>
        ConnectedUsers.ContainsKey(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsUserOnlineFlag(string userId) => ConnectedUsers.ContainsKey(userId);

    public async Task AddUser(string userId, string connectionId)
    {
        try
        {
            await _signalRSemaphore.WaitAsync();
            if (ConnectedUsers.ContainsKey(userId))
            {
                _ = ConnectedUsers[userId].Add(connectionId);
            }
            else
            {
                ConnectedUsers.Add(userId, new HashSet<string>() { connectionId });
            }
            _ = _signalRSemaphore.Release();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SignalRCache)} {nameof(AddUser)} {ex.GetType()}");
        }
        finally
        {
            _ = _signalRSemaphore.Release();
        }
    }

    public async Task RemoveUserConnection(string userId, string connectionId)
    {
        try
        {
            await _signalRSemaphore.WaitAsync();
            if (ConnectedUsers.ContainsKey(userId))
            {
                _ = ConnectedUsers[userId].Remove(connectionId);
            }
            _ = _signalRSemaphore.Release();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SignalRCache)} {nameof(RemoveUserConnection)} {ex.GetType()}");
        }
        finally
        {
            _ = _signalRSemaphore.Release();
        }
    }
}
