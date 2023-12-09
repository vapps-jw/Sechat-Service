using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services.CacheServices;

public class SignalRCache
{
    private readonly SemaphoreSlim _signalRChatSemaphore;
    private readonly SemaphoreSlim _signalRGamesSemaphore;
    private readonly ILogger<SignalRCache> _logger;

    public Dictionary<string, HashSet<string>> ConnectedChatUsers { get; set; } = new();
    public Dictionary<string, HashSet<string>> ConnectedGamesUsers { get; set; } = new();

    public SignalRCache(ILogger<SignalRCache> logger)
    {
        _signalRChatSemaphore = new SemaphoreSlim(1);
        _signalRGamesSemaphore = new SemaphoreSlim(1);
        _logger = logger;
    }

    // Chat

    public string IsChatUserOnline(string userId) =>
        ConnectedChatUsers.ContainsKey(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsChatUserOnlineFlag(string userId) => ConnectedChatUsers.ContainsKey(userId);

    public async Task AddChatUser(string userId, string connectionId)
    {
        try
        {
            await _signalRChatSemaphore.WaitAsync();
            if (ConnectedChatUsers.ContainsKey(userId))
            {
                _ = ConnectedChatUsers[userId].Add(connectionId);
            }
            else
            {
                ConnectedChatUsers.Add(userId, new HashSet<string>() { connectionId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{class} {method} {type}", nameof(SignalRCache), nameof(AddChatUser), ex.GetType());
        }
        finally
        {
            _ = _signalRChatSemaphore.Release();
        }
    }

    public async Task RemoveChatUserConnection(string userId, string connectionId)
    {
        try
        {
            await _signalRChatSemaphore.WaitAsync();
            if (ConnectedChatUsers.ContainsKey(userId))
            {
                _ = ConnectedChatUsers[userId].Remove(connectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{class} {method} {type}", nameof(SignalRCache), nameof(RemoveChatUserConnection), ex.GetType());
        }
        finally
        {
            _ = _signalRChatSemaphore.Release();
        }
    }

    // Games

    public string IsGamesUserOnline(string userId) =>
        ConnectedGamesUsers.ContainsKey(userId) ? AppConstants.ContactState.Online : AppConstants.ContactState.Offline;

    public bool IsGamesUserOnlineFlag(string userId) => ConnectedGamesUsers.ContainsKey(userId);

    public async Task AddGamesUser(string userId, string connectionId)
    {
        try
        {
            await _signalRGamesSemaphore.WaitAsync();
            if (ConnectedGamesUsers.ContainsKey(userId))
            {
                _ = ConnectedGamesUsers[userId].Add(connectionId);
            }
            else
            {
                ConnectedGamesUsers.Add(userId, new HashSet<string>() { connectionId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{class} {method} {type}", nameof(SignalRCache), nameof(AddChatUser), ex.GetType());
        }
        finally
        {
            _ = _signalRGamesSemaphore.Release();
        }
    }

    public async Task RemoveGamesUserConnection(string userId, string connectionId)
    {
        try
        {
            await _signalRGamesSemaphore.WaitAsync();
            if (ConnectedGamesUsers.ContainsKey(userId))
            {
                _ = ConnectedGamesUsers[userId].Remove(connectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{class} {method} {type}", nameof(SignalRCache), nameof(RemoveChatUserConnection), ex.GetType());
        }
        finally
        {
            _ = _signalRGamesSemaphore.Release();
        }
    }
}
