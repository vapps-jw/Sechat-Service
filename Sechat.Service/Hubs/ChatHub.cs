using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Hubs;

public interface IChatHub
{
    Task MessageIncoming(RoomMessageDto message);
    Task MessagesWereViewed(RoomUserActionMessage message);
    Task MessageWasViewed(RoomMessageUserActionMessage message);
    Task VideoCallDataIncoming(VideoData videoData);
    Task VideoCallRequested(StringMessage message);
    Task ICECandidateIncoming(StringMessage message);
    Task WebRTCOfferIncoming(StringMessageForUser message);
    Task WebRTCAnswerIncoming(StringMessageForUser message);
    Task WebRTCExchangeCompleted(StringMessage message);
    Task VideoCallApproved(StringMessage message);
    Task VideoCallRejected(StringMessage message);
    Task VideoCallTerminated(StringMessage message);
    Task RoomDeleted(ResourceGuid message);
    Task ConnectionRequestReceived(UserContactDto message);
    Task ConnectionDeleted(ResourceId message);
    Task ConnectionUpdated(UserContactDto message);
    Task RoomUpdated(RoomDto message);
    Task UserAddedToRoom(RoomDto message);
    Task UserRemovedFromRoom(RoomUserActionMessage message);
}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly UserRepository _userRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PushNotificationService _pushNotificationService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly IEncryptor _encryptor;
    private readonly ChatRepository _chatRepository;

    public ChatHub(
        UserRepository userRepository,
        UserManager<IdentityUser> userManager,
        PushNotificationService pushNotificationService,
        ILogger<ChatHub> logger,
        IMapper mapper,
        IEncryptor encryptor,
        ChatRepository chatRepository)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
        _mapper = mapper;
        _encryptor = encryptor;
        _chatRepository = chatRepository;
    }

    public void LogConnection(StringMessage connectionEstablishedDto) =>
        _logger.LogWarning("Connection established for user Id: {0} Name: {1} Message: {2}", UserId, UserName, connectionEstablishedDto.Message);

    [Obsolete]
    public async Task SendVideoCallData(IAsyncEnumerable<VideoData> videoData)
    {
        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        await foreach (var d in videoData)
        {
            var user = await _userManager.FindByNameAsync(d.UserName);
            if (user is not null)
            {
                if (userContacts.Any(uc => uc.Equals(user.Id)))
                {
                    await Clients.Group(user.Id).VideoCallDataIncoming(d);
                }
            }
        }
    }

    public async Task SendICECandidate(StringMessageForUser message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).ICECandidateIncoming(new StringMessage(message.Message));
    }

    public async Task SendWebRTCOffer(StringMessageForUser message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCOfferIncoming(new StringMessageForUser(UserName, message.Message));
    }

    public async Task SendWebRTCExchangeCompleted(StringMessage message)
    {
        var contactId = await IsContactAllowed(message.Message);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCExchangeCompleted(new StringMessage(UserName));
    }

    public async Task SendWebRTCAnswer(StringMessageForUser message)
    {
        var contactId = await IsContactAllowed(message.UserName);
        if (string.IsNullOrEmpty(contactId)) return;

        await Clients.Group(contactId).WebRTCAnswerIncoming(new StringMessageForUser(UserName, message.Message));
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
            await Groups.AddToGroupAsync(Context.ConnectionId, UserId);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
        _ = base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception) => base.OnDisconnectedAsync(exception);
}
