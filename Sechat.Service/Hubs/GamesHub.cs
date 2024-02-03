using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Services;
using Sechat.Service.Services.CacheServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Sechat.Service.Configuration.AppConstants;

namespace Sechat.Service.Hubs;
public interface IGamesHub
{

}

[Authorize]
[Authorize(AppConstants.AuthorizationPolicy.ChatPolicy)]
public class GamesHub : SechatHubBase<IGamesHub>
{
    private readonly Channel<DefaultNotificationDto> _pushNotificationChannel;
    private readonly SignalRCache _signalRConnectionsMonitor;
    private readonly UserRepository _userRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly IHostEnvironment _env;
    private readonly IEmailClient _emailClient;
    private readonly ChatRepository _chatRepository;

    public GamesHub(
        Channel<DefaultNotificationDto> pushNotificationChannel,
        SignalRCache signalRConnectionsMonitor,
        UserRepository userRepository,
        UserManager<IdentityUser> userManager,
        ILogger<GamesHub> logger,
        IMapper mapper,
        IHostEnvironment env,
        IEmailClient emailClient,
        ChatRepository chatRepository)
    {
        _pushNotificationChannel = pushNotificationChannel;
        _signalRConnectionsMonitor = signalRConnectionsMonitor;
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = (ILogger<ChatHub>)logger;
        _mapper = mapper;
        _env = env;
        _emailClient = emailClient;
        _chatRepository = chatRepository;
    }

    private async Task<string> IsContactAllowed(string userName)
    {
        var contact = await _userManager.FindByNameAsync(userName);
        if (contact is null) return string.Empty;

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        return !userContacts.Any(c => c.Equals(contact.Id)) ? string.Empty : contact.Id;
    }

    public void LogConnection(StringMessage connectionEstablishedDto) =>
        _logger.LogWarning("Connection established for user Id: {UserId} Name: {UserName} Message: {Message}", UserId, UserName, connectionEstablishedDto.Message);

    public async Task<StringMessage> CheckOnlineState(StringMessage message)
    {
        try
        {
            var contactId = await IsContactAllowed(message.Message);
            return string.IsNullOrEmpty(contactId) ? null : new StringMessage(ContactState.Online);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userContacts = await _userRepository.GetAllowedContactsIds(UserId);

            await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
            await _signalRConnectionsMonitor.AddGamesUser(UserId, Context.ConnectionId);

            var tasks = new List<Task>();
            foreach (var userContact in userContacts)
            {
                tasks.Add(Clients.Group(userContact).ContactStateChanged(new StringUserMessage(UserName, ContactState.Online)));
            }

            await Task.WhenAll(tasks);
            _ = base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            _ = await _emailClient.SendExceptionNotificationAsync(ex);
            if (_env.IsProduction())
            {
                _ = await _emailClient.SendExceptionNotificationAsync(ex);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var userContacts = await _userRepository.GetAllowedContactsIds(UserId);

            await _signalRConnectionsMonitor.RemoveGamesUserConnection(UserId, Context.ConnectionId);

            if (!_signalRConnectionsMonitor.IsChatUserOnlineFlag(UserId))
            {
                var tasks = new List<Task>();
                foreach (var userContact in userContacts)
                {
                    tasks.Add(Clients.Group(userContact).ContactStateChanged(new StringUserMessage(UserName, ContactState.Offline)));
                }
                await Task.WhenAll(tasks);
            }
            _ = base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            if (_env.IsProduction())
            {
                _ = await _emailClient.SendExceptionNotificationAsync(ex);
            }
        }
    }
}
