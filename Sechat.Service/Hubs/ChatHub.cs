using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Sechat.Service.Dtos.SignalRDtos;

namespace Sechat.Service.Hubs;

public interface IChatHub
{

}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger) => _logger = logger;

    public void LogConnection(ConnectionEstablishedDto data) => _logger.LogWarning("Connection established for user Id: {0} Name: {1} Message: {2}", UserId, UserName, data.Message);
}
