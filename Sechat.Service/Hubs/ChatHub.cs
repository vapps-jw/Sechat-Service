using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.SignalRDtos;
using Sechat.Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sechat.Service.Hubs;

public interface IChatHub
{
    Task MessageIncoming(RoomMessageDto message);
    Task RoomDeleted(RoomIdMessage message);
}

[Authorize]
public class ChatHub : SechatHubBase<IChatHub>
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IMapper _mapper;
    private readonly IEncryptor _encryptor;
    private readonly ChatRepository _chatRepository;

    public ChatHub(
        ILogger<ChatHub> logger,
        IMapper mapper,
        IEncryptor encryptor,
        ChatRepository chatRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _encryptor = encryptor;
        _chatRepository = chatRepository;
    }

    public void LogConnection(ConnectionEstablished connectionEstablishedDto) =>
        _logger.LogWarning("Connection established for user Id: {0} Name: {1} Message: {2}", UserId, UserName, connectionEstablishedDto.Message);

    public async Task<RoomDto> CreateRoom(RoomNameMessage request)
    {
        try
        {
            var newRoom = _chatRepository.CreateRoom(request.RoomName, UserId, _encryptor.GenerateKey());
            if (await _chatRepository.SaveChanges() == 0)
            {
                throw new Exception("Room creation failed");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, newRoom.Id);
            var responseDto = _mapper.Map<RoomDto>(newRoom);

            await Groups.AddToGroupAsync(Context.ConnectionId, responseDto.Id);

            return responseDto;
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

    public async Task<RoomIdMessage> ConnectToRoom(RoomIdMessage message)
    {
        try
        {
            if (_chatRepository.IsRoomAllowed(UserId, message.RoomId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, message.RoomId);
                return new RoomIdMessage(message.RoomId);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task SendMessage(IncomingMessage incomingMessageDto)
    {
        try
        {
            if (!_chatRepository.IsRoomAllowed(UserId, incomingMessageDto.RoomId))
            {
                throw new Exception("You dont have access to this room");
            }

            var res = _chatRepository.CreateMessage(UserId, incomingMessageDto.Text, incomingMessageDto.RoomId);
            _ = await _chatRepository.SaveChanges();
            var messageDto = _mapper.Map<RoomMessageDto>(res);

            await Clients.Group(incomingMessageDto.RoomId).MessageIncoming(messageDto);
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task SendRoomDeleted(string roomId)
    {
        try
        {
            await Clients.Group(roomId).RoomDeleted(new RoomIdMessage(roomId));
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public void SendInviteUser(string userName)
    {
        try
        {


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
}
