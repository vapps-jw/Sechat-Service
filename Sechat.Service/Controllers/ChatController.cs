using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Sechat.Data.QueryModels;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class ChatController : SechatControllerBase
{
    private readonly IOptionsMonitor<CryptographySettings> _cryptoSettings;
    private readonly CryptographyService _cryptographyService;
    private readonly SignalRConnectionsMonitor _signalRConnectionsMonitor;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly ChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public ChatController(
        IOptionsMonitor<CryptographySettings> cryptoSettings,
        CryptographyService cryptographyService,
        SignalRConnectionsMonitor signalRConnectionsMonitor,
        UserManager<IdentityUser> userManager,
        UserRepository userRepository,
        ChatRepository chatRepository,
        IMapper mapper,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _cryptoSettings = cryptoSettings;
        _cryptographyService = cryptographyService;
        _signalRConnectionsMonitor = signalRConnectionsMonitor;
        _userManager = userManager;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _mapper = mapper;
        _chatHubContext = chatHubContext;
    }

    //[Obsolete("To be removed")]
    //[HttpGet("get-state")]
    //public async Task<IActionResult> GetState()
    //{
    //    var rooms = await _chatRepository.GetRoomsWithMessages(UserId);
    //    var contacts = await _userRepository.GetContacts(UserId);

    //    var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //    var iv = Array.ConvertAll(parts, byte.Parse);

    //    var e2e = Request.Cookies[AppConstants.Cookies.E2E];
    //    var e2eData = JsonSerializer.Deserialize<E2EData[]>(e2e);

    //    foreach (var room in rooms)
    //    {
    //        var roomKey = e2eData.FirstOrDefault(k => k.Key.Equals(room.Id));

    //        foreach (var message in room.Messages)
    //        {
    //            if (!room.EncryptedByUser)
    //            {
    //                message.Text = _cryptographyService.Decrypt(message.Text, room.RoomKey, iv);
    //            }
    //            else
    //            {
    //                if (roomKey is not null)
    //                {
    //                    message.Text = _cryptographyService.Encrypt(message.Text, roomKey.Key);
    //                }
    //            }

    //            foreach (var viewer in message.MessageViewers)
    //            {
    //                viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
    //            }
    //        }
    //    }

    //    var roomDtos = _mapper.Map<List<RoomDto>>(rooms);

    //    foreach (var room in roomDtos)
    //    {
    //        foreach (var message in room.Messages)
    //        {
    //            foreach (var viewer in message.MessageViewers)
    //            {
    //                if (viewer.User.Equals(UserName))
    //                {
    //                    message.WasViewed = true;
    //                    continue;
    //                }
    //            }
    //        }
    //    }

    //    var contactDtos = _mapper.Map<List<ContactDto>>(contacts);
    //    var connectedContacts = contacts
    //        .Where(c => _signalRConnectionsMonitor.ConnectedUsers.Any(cu => cu.Equals(c.InvitedId) && c.InvitedId != UserId) ||
    //                    _signalRConnectionsMonitor.ConnectedUsers.Any(cu => cu.Equals(c.InviterId) && c.InviterId != UserId))
    //        .Select(c => c.Id)
    //        .ToList();

    //    foreach (var contactDto in contactDtos)
    //    {
    //        contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
    //            AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
    //    }

    //    var res = new StateDto
    //    {
    //        Rooms = roomDtos,
    //        UserContacts = contactDtos
    //    };

    //    return Ok(res);
    //}

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var contacts = await _userRepository.GetContacts(UserId);
        var contactDtos = _mapper.Map<List<ContactDto>>(contacts);
        var connectedContacts = contacts
            .Where(c => _signalRConnectionsMonitor.ConnectedUsers.Any(cu => cu.Equals(c.InvitedId) && c.InvitedId != UserId) ||
                        _signalRConnectionsMonitor.ConnectedUsers.Any(cu => cu.Equals(c.InviterId) && c.InviterId != UserId))
            .Select(c => c.Id)
            .ToList();

        foreach (var contactDto in contactDtos)
        {
            contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
                AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
        }

        return Ok(contactDtos);
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _chatRepository.GetRoomsWithMessages(UserId);
        var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var iv = Array.ConvertAll(parts, byte.Parse);

        foreach (var room in rooms)
        {
            var roomKey = ExtractE2ECookieDataForRoom(room.Id);
            foreach (var message in room.Messages)
            {
                if (!room.EncryptedByUser)
                {
                    message.Text = _cryptographyService.Decrypt(message.Text, room.RoomKey, iv);
                }
                else
                {
                    if (roomKey is not null)
                    {
                        message.Text = _cryptographyService.Decrypt(message.Text, roomKey.Key);
                    }
                }

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

        return Ok(roomDtos);
    }

    [HttpPost("rooms-update")]
    public async Task<IActionResult> GetRoomsUpdate([FromBody] List<GetRoomUpdate> getRoomUpdates)
    {
        var rooms = await _chatRepository.GetRoomsWithMessages(UserId, getRoomUpdates);

        foreach (var room in rooms)
        {
            foreach (var message in room.Messages)
            {
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

        return Ok(roomDtos);
    }

    [HttpGet("get-state-updates")]
    public IActionResult GetStateUpdates() => Ok();

    [HttpGet("new-messages")]
    public async Task<IActionResult> GetNewMessages([FromBody] GetNewMessagesRequest getNewMessagesRequest)
    {
        if (!_chatRepository.IsRoomsMember(UserId, getNewMessagesRequest.LastMessageInTheRooms.Select(lm => lm.RoomId).ToList()))
        {
            return BadRequest("Not your room?");
        }

        var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var iv = Array.ConvertAll(parts, byte.Parse);

        var res = new List<RoomDto>();
        foreach (var lastMessageInTheRoom in getNewMessagesRequest.LastMessageInTheRooms)
        {
            var room = await _chatRepository.GetRoomWithNewMessages(lastMessageInTheRoom.RoomId, lastMessageInTheRoom.LastMessage);
            if (!room.Messages.Any()) continue;
            if (!room.EncryptedByUser)
            {
                room.Messages.ForEach(m => m.Text = _cryptographyService.Decrypt(m.Text, room.RoomKey, iv));
            }
            else
            {
                var roomKey = ExtractE2ECookieDataForRoom(room.Id);
                if (roomKey is null)
                {
                    room.Messages = new();
                }
                else
                {
                    room.Messages.ForEach(m => m.Text = _cryptographyService.Decrypt(m.Text, roomKey.Key));
                }
            }

            res.Add(_mapper.Map<RoomDto>(room));
        }

        return Ok(res);
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage(
        [FromServices] Channel<DefaultNotificationDto> channel,
        [FromBody] IncomingMessage incomingMessageDto)
    {
        if (!_chatRepository.IsRoomAllowed(UserId, incomingMessageDto.RoomId))
        {
            throw new Exception("You dont have access to this room");
        }

        var messageToSave = incomingMessageDto.Text;
        var messageToSendBack = string.Empty;

        if (_chatRepository.RoomEncryptedByUser(incomingMessageDto.RoomId))
        {
            var roomKey = ExtractE2ECookieDataForRoom(incomingMessageDto.RoomId);
            if (roomKey is null) return BadRequest("Something is wrong with this room");

            var encryptedMessage = _cryptographyService.Encrypt(incomingMessageDto.Text, roomKey.Key);

            messageToSave = encryptedMessage;
            messageToSendBack = encryptedMessage;
        }
        else
        {
            var roomKey = _chatRepository.GetRoomKey(incomingMessageDto.RoomId);

            var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var iv = Array.ConvertAll(parts, byte.Parse);

            messageToSave = _cryptographyService.Encrypt(incomingMessageDto.Text, roomKey, iv);
            messageToSendBack = incomingMessageDto.Text;
        }

        var res = _chatRepository.CreateMessage(UserId, messageToSave, incomingMessageDto.RoomId);
        if (await _chatRepository.SaveChanges() == 0)
        {
            return BadRequest("Message not sent");
        }

        res.Text = messageToSendBack;
        var messageDto = _mapper.Map<RoomMessageDto>(res);

        foreach (var viewer in messageDto.MessageViewers)
        {
            viewer.User = (await _userManager.FindByIdAsync(viewer.User))?.UserName;
        }

        await _chatHubContext.Clients.Group(incomingMessageDto.RoomId).MessageIncoming(messageDto);

        var roomMembers = _chatRepository.GetRoomMembersIds(incomingMessageDto.RoomId);
        _ = roomMembers.RemoveAll(m => m.Equals(UserId));
        if (!roomMembers.Any()) return Ok();

        var roomName = _chatRepository.GetRoomName(incomingMessageDto.RoomId);
        foreach (var member in roomMembers)
        {
            await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.IncomingMessage, member, roomName));
        }

        return Ok();
    }

    [HttpPatch("messages-viewed")]
    public async Task<IActionResult> MessagesViewed([FromBody] ResourceGuid resourceGuid)
    {
        if (!_chatRepository.IsRoomMember(UserId, resourceGuid.Id)) return BadRequest("Not your room");

        _chatRepository.MarkMessagesAsViewed(UserId, resourceGuid.Id);

        if (await _chatRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(resourceGuid.Id).MessagesWereViewed(new RoomUserActionMessage(resourceGuid.Id, UserName));
        }

        return Ok();
    }

    [HttpPatch("message-viewed/{roomId}/{messageId}")]
    public async Task<IActionResult> MessageViewed(string roomId, long messageId)
    {
        if (!_chatRepository.IsRoomMember(UserId, roomId)) return BadRequest("Not your room");

        _chatRepository.MarkMessageAsViewed(UserId, messageId);

        if (await _chatRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(roomId).MessageWasViewed(new RoomMessageUserActionMessage(roomId, messageId, UserName));
        }

        return Ok();
    }

    [HttpPost("add-to-room")]
    public async Task<IActionResult> AddToRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate)
    {
        var user = await _userManager.FindByNameAsync(roomMemberUpdate.UserName);
        if (!_userRepository.ContactExists(roomMemberUpdate.connectionId, UserId, user.Id)) return BadRequest("This is not your friend");

        var contact = await _userRepository.GetContact(roomMemberUpdate.connectionId);
        if (contact.Blocked) return BadRequest("User blocked");

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
        if (!_userRepository.ContactExists(roomMemberUpdate.connectionId, UserId, user.Id)) return BadRequest("Not your friend");

        var room = _chatRepository.RemoveFromRoom(roomMemberUpdate.RoomId, user.Id);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(roomMemberUpdate.RoomId).UserRemovedFromRoom(new RoomUserActionMessage(roomDto.Id, roomMemberUpdate.UserName));
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
            await _chatHubContext.Clients.Group(roomMemberUpdate.RoomId).UserRemovedFromRoom(new RoomUserActionMessage(roomDto.Id, UserName));
            return Ok();
        }

        return BadRequest();
    }

    [HttpDelete("delete-room")]
    public async Task<IActionResult> DeleteRoom(string roomId)
    {
        if (await _chatRepository.DeleteRoom(roomId, UserId) > 0)
        {
            await _chatHubContext.Clients.Group(roomId).RoomDeleted(new ResourceGuid(roomId));
        }

        return Ok();
    }
}
