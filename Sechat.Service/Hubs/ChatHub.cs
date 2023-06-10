using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sechat.Data.DataServices;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Sechat.Service.Configuration.AppConstants;

namespace Sechat.Service.Hubs;

public interface IChatHub
{
    // Video call connection
    Task VideoCallRequested(StringMessage message);
    Task ICECandidateIncoming(StringMessage message);
    Task WebRTCOfferIncoming(StringUserMessage message);
    Task WebRTCAnswerIncoming(StringUserMessage message);
    Task WebRTCExchangeCompleted(StringMessage message);

    // Video call replies
    Task VideoCallApproved(StringMessage message);
    Task VideoCallDeclined(StringMessage message);
    Task VideoCallRejected(StringMessage message);
    Task VideoCallTerminated(StringMessage message);

    // Video call media
    Task MicStateChanged(StringMessage message);
    Task CamStateChanged(StringMessage message);

    // Chat Messages
    Task MessageIncoming(RoomMessageDto message);
    Task MessagesWereViewed(RoomUserActionMessage message);
    Task MessageWasViewed(RoomMessageUserActionMessage message);

    // Chat Rooms
    Task RoomDeleted(ResourceGuid message);
    Task RoomUpdated(RoomDto message);
    Task UserAddedToRoom(RoomDto message);
    Task UserRemovedFromRoom(RoomUserActionMessage message);

    // Chat Contacts
    Task ConnectionRequestReceived(UserContactDto message);
    Task ConnectionDeleted(ResourceId message);
    Task ConnectionUpdated(UserContactDto message);
    Task ContactStateChanged(StringUserMessage message);
}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly Channel<DefaultNotificationDto> _pushNotificationChannel;
    private readonly SignalRConnectionsMonitor _signalRConnectionsMonitor;
    private readonly UserRepository _userRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly DataEncryptor _encryptor;
    private readonly ChatRepository _chatRepository;

    public ChatHub(
        Channel<DefaultNotificationDto> pushNotificationChannel,
        SignalRConnectionsMonitor signalRConnectionsMonitor,
        UserRepository userRepository,
        UserManager<IdentityUser> userManager,
        ILogger<ChatHub> logger,
        IMapper mapper,
        DataEncryptor encryptor,
        ChatRepository chatRepository)
    {
        _pushNotificationChannel = pushNotificationChannel;
        _signalRConnectionsMonitor = signalRConnectionsMonitor;
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
        _mapper = mapper;
        _encryptor = encryptor;
        _chatRepository = chatRepository;
    }

    public void LogConnection(StringMessage connectionEstablishedDto) =>
        _logger.LogWarning("Connection established for user Id: {0} Name: {1} Message: {2}", UserId, UserName, connectionEstablishedDto.Message);

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

    public async Task SendMicStateChange(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).MicStateChanged(new StringMessage(message.Message));
    }

    public async Task SendCamStateChange(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).CamStateChanged(new StringMessage(message.Message));
    }

    public async Task SendICECandidate(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).ICECandidateIncoming(new StringMessage(message.Message));
    }

    public async Task SendWebRTCOffer(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCOfferIncoming(new StringUserMessage(UserName, message.Message));
    }

    public async Task SendWebRTCExchangeCompleted(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCExchangeCompleted(new StringMessage(UserName));
    }

    public async Task SendWebRTCAnswer(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCAnswerIncoming(new StringUserMessage(UserName, message.Message));
    }

    public async Task RejectVideoCall(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;
        await Clients.Group(contactId).VideoCallRejected(new StringMessage(UserName));
    }

    public async Task TerminateVideoCall(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;
        await Clients.Group(contactId).VideoCallTerminated(new StringMessage(UserName));
    }

    public async Task ApproveVideoCall(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).VideoCallApproved(new StringMessage(UserName));
    }

    public async Task VideoCallRequest(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;

        await _pushNotificationChannel.Writer.WriteAsync(new DefaultNotificationDto(PushNotificationType.IncomingVideoCall, contactId, UserName));
        await Clients.Group(contactId).VideoCallRequested(new StringMessage(UserName));
    }

    public async Task<RoomDto> CreateRoom(RoomNameMessage request)
    {
        try
        {
            var newRoom = _chatRepository.CreateRoom(request.RoomName, UserId, UserName, _encryptor.GenerateKey());
            if (await _chatRepository.SaveChanges() == 0)
            {
                throw new Exception("Room creation failed");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, newRoom.Id);
            return _mapper.Map<RoomDto>(newRoom);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task<RoomIdsMessage> ConnectToRooms(RoomIdsMessage connectToRoomsRequest)
    {
        try
        {
            var result = new List<string>();
            foreach (var request in connectToRoomsRequest.RoomIds)
            {
                if (_chatRepository.IsRoomAllowed(UserId, request))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, request);
                    result.Add(request);
                }
            }
            return new RoomIdsMessage(result);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task<ResourceGuid> ConnectToRoom(ResourceGuid message)
    {
        try
        {
            if (_chatRepository.IsRoomAllowed(UserId, message.Id))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, message.Id);
                return new ResourceGuid(message.Id);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task<ResourceGuid> DisconnectFromRoom(ResourceGuid message)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, message.Id);
            return new ResourceGuid(message.Id);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    private async Task<string> IsContactAllowed(string userName)
    {
        var contact = await _userManager.FindByNameAsync(userName);
        if (contact is null) return string.Empty;

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        return !userContacts.Any(c => c.Equals(contact.Id)) ? string.Empty : contact.Id;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
            foreach (var userContact in userContacts)
            {
                await Clients.Group(userContact).ContactStateChanged(new StringUserMessage(UserName, ContactState.Online));
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
            _signalRConnectionsMonitor.ConnectedUsers.Add(UserId);
            _ = base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        foreach (var userContact in userContacts)
        {
            await Clients.Group(userContact).ContactStateChanged(new StringUserMessage(UserName, ContactState.Offline));
        }

        _ = _signalRConnectionsMonitor.ConnectedUsers.Remove(UserId);
        _ = base.OnDisconnectedAsync(exception);
    }
}
