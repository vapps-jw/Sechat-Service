using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.CryptoDtos;
using Sechat.Service.Dtos.Messages;
using Sechat.Service.Services.CacheServices;
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
    Task ScreenShareStateChanged(StringMessage message);

    // Messages General
    Task UserTypingInRoom(RoomMessageTypingUser message);
    Task UserTypingDirectMessage(DirectMessageTypingUser message);

    // Chat Messages
    Task MessageIncoming(MessageDto message);
    Task MessagesWereViewed(RoomUserActionMessage message);
    Task MessageWasViewed(RoomMessageUserActionMessage message);
    Task MessageDeleted(MessageId message);

    // Chat Direct Messages
    Task DirectMessageIncoming(DirectMessageDto message);
    Task DirectMessagesWereViewed(DirectMessagesViewed message);
    Task DirectMessageWasViewed(DirectMessageViewed message);
    Task DirectMessageDeleted(DirectMessageId message);
    Task ContactUpdateRequired(ContactUpdateRequired message);

    // Chat Rooms
    Task RoomDeleted(ResourceGuid message);
    Task RoomUpdated(RoomDto message);
    Task UserAddedToRoom(RoomDto message);
    Task UserRemovedFromRoom(RoomUserActionMessage message);

    // Chat Contacts
    Task ContactRequestReceived(ContactDto message);
    Task ContactDeleted(ResourceId message);
    Task ContactUpdated(ContactDto message);
    Task ContactStateChanged(StringUserMessage message);

    // E2E

    Task DMKeyRequested(DMKeyRequest keyRequest);
    Task DMKeyIncoming(DMSharedKey key);

    Task RoomKeyRequested(RoomKeyRequest keyRequest);
    Task RoomKeyIncoming(RoomSharedKey key);

    Task MasterKeyRequested();
    Task MasterKeyIncoming(MasterSharedKey key);
}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly Channel<DefaultNotificationDto> _pushNotificationChannel;
    private readonly SignalRCache _signalRConnectionsMonitor;
    private readonly UserRepository _userRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly ChatRepository _chatRepository;

    public ChatHub(
        Channel<DefaultNotificationDto> pushNotificationChannel,
        SignalRCache signalRConnectionsMonitor,
        UserRepository userRepository,
        UserManager<IdentityUser> userManager,
        ILogger<ChatHub> logger,
        IMapper mapper,

        ChatRepository chatRepository)
    {
        _pushNotificationChannel = pushNotificationChannel;
        _signalRConnectionsMonitor = signalRConnectionsMonitor;
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
        _mapper = mapper;

        _chatRepository = chatRepository;
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

    // Video Calls

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

    public async Task SendScreenShareStateChange(StringUserMessage message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).ScreenShareStateChanged(new StringMessage(message.Message));
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

    // Messages

    // Rooms

    public async Task ImTypingDirectMessage(ResourceId contactData)
    {
        try
        {
            var check = _userRepository.CheckContact(contactData.Id, out var contact);
            if (!check)
            {
                return;
            }

            var recipientId = contact.InviterId.Equals(UserId) ? contact.InvitedId : contact.InviterId;

            if (!_signalRConnectionsMonitor.IsUserOnlineFlag(recipientId))
            {
                return;
            }

            await Clients.Group(recipientId).UserTypingDirectMessage(new DirectMessageTypingUser(contact.Id, UserName));

        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task ImTypingRoomMessage(ResourceGuid roomData)
    {
        try
        {
            if (!_chatRepository.IsRoomMember(UserId, roomData.Id))
            {
                return;
            }

            var excluded = _signalRConnectionsMonitor.ConnectedUsers[UserId];
            await Clients.GroupExcept(roomData.Id, excluded).UserTypingInRoom(new RoomMessageTypingUser(roomData.Id, UserName));
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task<RoomDto> CreateRoom(CreateRoomMessage request)
    {
        try
        {
            var newRoom = _chatRepository.CreateRoom(
                request.RoomName,
                UserId,
                UserName);
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
                if (_chatRepository.IsRoomMember(UserId, request))
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
            if (_chatRepository.IsRoomMember(UserId, message.Id))
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

    // Contacts

    private async Task<string> IsContactAllowed(string userName)
    {
        var contact = await _userManager.FindByNameAsync(userName);
        if (contact is null) return string.Empty;

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        return !userContacts.Any(c => c.Equals(contact.Id)) ? string.Empty : contact.Id;
    }

    // E2E

    public async Task RequestDMKey(DMKeyRequest keyRequest)
    {
        try
        {
            var contactId = keyRequest.KeyHolder.Equals(UserName) ? UserId : await IsContactAllowed(keyRequest.KeyHolder);

            if (string.IsNullOrEmpty(contactId)) return;
            await Clients.Group(contactId).DMKeyRequested(keyRequest);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task ShareDMKey(DMSharedKey key)
    {
        try
        {
            var contactId = key.Receipient.Equals(UserName) ? UserId : await IsContactAllowed(key.Receipient);

            if (string.IsNullOrEmpty(contactId)) return;
            await Clients.Group(contactId).DMKeyIncoming(key);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task RequestRoomKey(RoomKeyRequest keyRequest)
    {
        try
        {
            if (!_chatRepository.IsRoomMember(UserId, keyRequest.Id))
            {
                return;
            }

            var roomMembers = _chatRepository.GetRoomMembersIds(keyRequest.Id);
            _ = roomMembers.RemoveAll(rm => !_signalRConnectionsMonitor.IsUserOnlineFlag(rm));
            foreach (var roomMember in roomMembers)
            {
                await Clients.Group(roomMember).RoomKeyRequested(keyRequest);
            }
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task ShareRoomKey(RoomSharedKey key)
    {
        try
        {
            var needKey = await _userManager.FindByNameAsync(key.Receipient);
            if (!_chatRepository.IsRoomMember(needKey.Id, key.Id))
            {
                return;
            }

            await Clients.Group(needKey.Id).RoomKeyIncoming(key);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task RequestMasterKey()
    {
        try
        {
            await Clients.Group(UserId).MasterKeyRequested();
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task ShareMasterKey(MasterSharedKey key)
    {
        try
        {
            await Clients.Group(UserId).MasterKeyIncoming(key);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    // Connections

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userContacts = await _userRepository.GetAllowedContactsIds(UserId);

            await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
            await _signalRConnectionsMonitor.AddUser(UserId, Context.ConnectionId);

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
            throw new HubException(ex.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);

        await _signalRConnectionsMonitor.RemoveUserConnection(UserId, Context.ConnectionId);

        if (!_signalRConnectionsMonitor.IsUserOnlineFlag(UserId))
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
}
