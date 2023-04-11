﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class ChatController : SechatControllerBase
{
    private readonly PushNotificationService _pushNotificationService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly ChatRepository _chatRepository;
    private readonly IEncryptor _encryptor;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public ChatController(
        PushNotificationService pushNotificationService,
        UserManager<IdentityUser> userManager,
        UserRepository userRepository,
        ChatRepository chatRepository,
        IEncryptor encryptor,
        IMapper mapper,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _pushNotificationService = pushNotificationService;
        _userManager = userManager;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _encryptor = encryptor;
        _mapper = mapper;
        _chatHubContext = chatHubContext;
    }

    [HttpGet("get-state")]
    public async Task<IActionResult> GetState()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var connections = await _userRepository.GetConnections(UserId);

        foreach (var room in rooms)
        {
            foreach (var message in room.Messages)
            {
                message.Text = _encryptor.DecryptString(room.RoomKey, message.Text);
                foreach (var viewer in message.MessageViewers)
                {
                    viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
                }
            }
        }

        var roomDtos = _mapper.Map<List<RoomDto>>(rooms);

        foreach (var room in roomDtos)
        {
            foreach (var message in room.Messages)
            {
                foreach (var viewer in message.MessageViewers)
                {
                    if (viewer.User.Equals(UserName))
                    {
                        message.WasViewed = true;
                        continue;
                    }
                }
            }
        }

        var connectionDtos = _mapper.Map<List<UserConnectionDto>>(connections);

        var res = new StateDto
        {
            Rooms = roomDtos,
            UserConnections = connectionDtos
        };

        return Ok(res);
    }

    [HttpGet("get-my-rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _chatRepository.GetRooms(UserId);
        var responseDtos = _mapper.Map<List<RoomDto>>(rooms);

        return Ok(responseDtos);
    }

    [HttpGet("new-messages")]
    public async Task<IActionResult> GetNewMessages([FromBody] GetNewMessagesRequest getNewMessagesRequest)
    {
        if (!_chatRepository.IsRoomsMember(UserId, getNewMessagesRequest.LastMessageInTheRooms.Select(lm => lm.RoomId).ToList()))
        {
            return BadRequest("Not your room?");
        }

        var res = new List<RoomDto>();
        foreach (var lastMessageInTheRoom in getNewMessagesRequest.LastMessageInTheRooms)
        {
            var room = await _chatRepository.GetRoomWithNewMessages(lastMessageInTheRoom.RoomId, lastMessageInTheRoom.LastMessage);
            if (!room.Messages.Any()) continue;

            room.Messages.ForEach(m => m.Text = _encryptor.DecryptString(room.RoomKey, m.Text));

            res.Add(_mapper.Map<RoomDto>(room));
        }

        return Ok(res);
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] IncomingMessage incomingMessageDto)
    {
        if (!_chatRepository.IsRoomAllowed(UserId, incomingMessageDto.RoomId))
        {
            throw new Exception("You dont have access to this room");
        }

        var room = _chatRepository.GetRoomWithoutRelations(incomingMessageDto.RoomId);
        var encryptedMessage = new IncomingMessage(_encryptor.EncryptString(room.RoomKey, incomingMessageDto.Text), incomingMessageDto.RoomId);
        var roomMembers = _chatRepository.GetRoomMembers(incomingMessageDto.RoomId);
        _ = roomMembers.RemoveAll(m => m.Equals(UserId));

        var res = _chatRepository.CreateMessage(UserId, encryptedMessage.Text, encryptedMessage.RoomId);
        if (await _chatRepository.SaveChanges() == 0)
        {
            return BadRequest("Message not sent");
        }

        res.Text = incomingMessageDto.Text;
        var messageDto = _mapper.Map<RoomMessageDto>(res);

        foreach (var viewer in messageDto.MessageViewers)
        {
            viewer.User = (await _userManager.FindByIdAsync(viewer.User))?.UserName;
        }

        await _chatHubContext.Clients.Group(incomingMessageDto.RoomId).MessageIncoming(messageDto);

        if (!roomMembers.Any()) return Ok();

        foreach (var member in roomMembers)
        {
            await _pushNotificationService.IncomingMessageNotification(member, room.Name);
        }

        return Ok();
    }

    [HttpPatch("message-viewed")]
    public async Task<IActionResult> MessagesViewed([FromBody] ResourceGuid resourceGuid)
    {
        if (!_chatRepository.IsRoomMember(UserId, resourceGuid.Id)) return BadRequest("Not your room");

        _chatRepository.MarkMessagesAsViewed(UserId, resourceGuid.Id);
        _ = await _chatRepository.SaveChanges();
        return Ok();
    }

    [HttpPost("add-to-room")]
    public async Task<IActionResult> AddToRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate)
    {
        var user = await _userManager.FindByNameAsync(roomMemberUpdate.UserName);
        if (!_userRepository.ConnectionExists(roomMemberUpdate.connectionId, UserId, user.Id)) return BadRequest("This is not your friend");

        var connection = await _userRepository.GetConnection(roomMemberUpdate.connectionId);
        if (connection.Blocked) return BadRequest("User blocked");

        var room = _chatRepository.AddToRoom(roomMemberUpdate.RoomId, user.Id);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(user.Id).UserAddedToRoom(roomDto);
            await _chatHubContext.Clients.Group(roomMemberUpdate.RoomId).RoomUpdated(roomDto);
            return Ok();
        }

        return BadRequest("Room not updated");
    }

    [HttpPost("remove-from-room")]
    public async Task<IActionResult> RemoveFromRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate)
    {
        var user = await _userManager.FindByNameAsync(roomMemberUpdate.UserName);
        if (!_userRepository.ConnectionExists(roomMemberUpdate.connectionId, UserId, user.Id)) return BadRequest("Not your friend");

        var room = _chatRepository.RemoveFromRoom(roomMemberUpdate.RoomId, user.Id);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(roomMemberUpdate.RoomId).UserRemovedFromRoom(new UserRemovedFromRoom(roomDto.Id, roomMemberUpdate.UserName));
            return Ok();
        }

        return BadRequest();
    }

    [HttpPost("leave-room")]
    public async Task<IActionResult> LeaveRoom([FromBody] RoomRequest roomMemberUpdate)
    {
        if (!_chatRepository.IsRoomMember(UserId, roomMemberUpdate.RoomId)) return BadRequest("Not room member");

        var room = _chatRepository.RemoveFromRoom(roomMemberUpdate.RoomId, UserId);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(roomMemberUpdate.RoomId).UserRemovedFromRoom(new UserRemovedFromRoom(roomDto.Id, UserName));
            return Ok();
        }

        return BadRequest();
    }

    [HttpDelete("delete-room")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        _chatRepository.DeleteRoom(roomId, UserId);

        if (await _chatRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(roomId).RoomDeleted(new ResourceGuid(roomId));
        }

        return Ok();
    }
}
