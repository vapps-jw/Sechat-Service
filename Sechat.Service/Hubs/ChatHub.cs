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
    Task VideoCallDataIncoming(VideoData videoData);
    Task VideoCallRequested(StringMessage message);
    Task VideoCallApproved(StringMessage message);
    Task VideoCallRejected(StringMessage message);
    Task RoomDeleted(ResourceGuid message);
    Task ConnectionRequestReceived(UserConnectionDto message);
    Task ConnectionDeleted(ResourceId message);
    Task ConnectionUpdated(UserConnectionDto message);
    Task RoomUpdated(RoomDto message);
    Task UserAddedToRoom(RoomDto message);
    Task UserRemovedFromRoom(UserRemovedFromRoom message);
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

    public async Task SendVideoCallData(IAsyncEnumerable<VideoData> videoData)
    {
        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        await foreach (var d in videoData)
        {
            var userId = await _userManager.FindByNameAsync(d.UserName);
            if (userId is not null)
            {
                if (userContacts.Any(uc => uc.Equals(userId)))
                {
                    await Clients.Group(userId.Id).VideoCallDataIncoming(d);
                }
            }
        }
    }

    public async Task RejectVideoCall(StringMessage message)
    {
        var contact = await _userManager.FindByNameAsync(message.Message);
        if (contact is null)
        {
            return;
        }

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        if (!userContacts.Any(c => c.Equals(contact.Id)))
        {
            return;
        }

        await Clients.Group(contact.Id).VideoCallApproved(new StringMessage(UserName));
    }

    public async Task ApproveVideoCall(StringMessage message)
    {
        var contact = await _userManager.FindByNameAsync(message.Message);
        if (contact is null)
        {
            return;
        }

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        if (!userContacts.Any(c => c.Equals(contact.Id)))
        {
            return;
        }

        await Clients.Group(contact.Id).VideoCallRejected(new StringMessage(UserName));
    }

    public async Task VideoCallRequest(StringMessage message)
    {
        var contact = await _userManager.FindByNameAsync(message.Message);
        if (contact is null)
        {
            return;
        }

        var userContacts = await _userRepository.GetAllowedContactsIds(UserId);
        if (!userContacts.Any(c => c.Equals(contact.Id)))
        {
            return;
        }

        await Clients.Group(contact.Id).VideoCallRequested(new StringMessage(UserName));
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
