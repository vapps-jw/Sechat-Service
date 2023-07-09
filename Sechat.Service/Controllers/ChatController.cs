using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.UserDetails;
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

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var contacts = await _userRepository.GetContactsWithMessages(UserId);
        var contactDtos = PrepareContactDtos(contacts);

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
        var roomDtos = await PrepareRoomDtos(rooms);

        return Ok(roomDtos);
    }

    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetRoom(string roomId)
    {
        if (!_chatRepository.IsRoomAllowed(UserId, roomId))
        {
            return BadRequest("You dont have access to this room");
        }

        var rooms = await _chatRepository.GetRoomWithMessages(roomId);
        var roomDtos = await PrepareRoomDto(rooms);

        return Ok(roomDtos);
    }

    [HttpGet("contact/{contactId}")]
    public async Task<IActionResult> GetContact(long contactId)
    {
        if (!_userRepository.CheckContact(contactId, UserId, out _))
        {
            return BadRequest("Contact not allowed");
        }

        var contact = await _userRepository.GetContactWithMessages(contactId);
        var contactDto = PrepareContactDto(contact);

        return Ok(contactDto);
    }

    [HttpPost("rooms-update")]
    public async Task<IActionResult> GetRoomsUpdate([FromBody] List<GetRoomUpdate> getRoomUpdates)
    {
        var rooms = await _chatRepository.GetRoomsWithMessages(UserId, getRoomUpdates);
        var roomDtos = await PrepareRoomDtos(rooms);

        return Ok(roomDtos);
    }

    [HttpPost("contacts-update")]
    public async Task<IActionResult> GetContactsUpdate([FromBody] List<GetContactUpdate> getContactUpdates)
    {
        var contacts = await _userRepository.GetContactsWithMessages(UserId, getContactUpdates);
        var contactDtos = PrepareContactDtos(contacts);

        return Ok(contactDtos);
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage(
        [FromServices] Channel<DefaultNotificationDto> channel,
        [FromBody] IncomingMessage incomingMessageDto)
    {
        if (!_chatRepository.IsRoomAllowed(UserId, incomingMessageDto.RoomId))
        {
            return BadRequest("You dont have access to this room");
        }

        string messageToSave;
        string messageToSendBack;

        if (_chatRepository.RoomEncryptedByUser(incomingMessageDto.RoomId))
        {
            var e2eKey = ExtractE2ECookieDataForRoom(incomingMessageDto.RoomId);
            if (e2eKey is null) return BadRequest("Secret Key for this Room is missing");

            var encryptedMessage = _cryptographyService.Encrypt(incomingMessageDto.Text, e2eKey.Key);

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
        var messageDto = _mapper.Map<MessageDto>(res);

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

    [HttpPost("send-direct-message")]
    public async Task<IActionResult> SendDirectMessage(
    [FromServices] Channel<DefaultNotificationDto> channel,
    [FromBody] IncomingDirectMessage incomingMessageDto)
    {
        var recipient = await _userManager.FindByNameAsync(incomingMessageDto.Recipient);
        var contact = _userRepository.GetContact(UserId, recipient.Id);

        if (contact.Blocked) return BadRequest("User blocked");
        if (!contact.Approved) return BadRequest("Contact was not approved");

        if (string.IsNullOrEmpty(contact.ContactKey))
        {
            var newKey = _cryptographyService.GenerateStringKey();
            contact.ContactKey = newKey;
        }

        string messageToSave;
        string messageToSendBack;

        if (contact.EncryptedByUser)
        {
            var e2eKey = ExtractE2ECookieDataForContact(contact.Id);
            if (e2eKey is null) return BadRequest("Secret Key for this Chat is missing");
            var encryptedMessage = _cryptographyService.Encrypt(incomingMessageDto.Text, e2eKey.Key);

            messageToSave = encryptedMessage;
            messageToSendBack = encryptedMessage;
        }
        else
        {
            messageToSave = _cryptographyService.Encrypt(incomingMessageDto.Text, contact.ContactKey);
            messageToSendBack = incomingMessageDto.Text;
        }

        var res = _chatRepository.CreateDirectMessage(UserId, messageToSave, contact.Id);
        if (await _chatRepository.SaveChanges() == 0)
        {
            return BadRequest("Message not sent");
        }

        res.Text = messageToSendBack;
        var messageDto = _mapper.Map<DirectMessageDto>(res);

        await _chatHubContext.Clients.Group(UserId).DirectMessageIncoming(messageDto);
        await _chatHubContext.Clients.Group(recipient.Id).DirectMessageIncoming(messageDto);
        await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.IncomingDirectMessage, recipient.Id, UserName));

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

    [HttpDelete("message/{roomId}/{messageId}")]
    public async Task<IActionResult> DeleteMessage(string roomId, long messageId)
    {
        if (!_chatRepository.MessageExists(messageId)) return BadRequest("Message does not exits");
        if (!_chatRepository.IsRoomMember(UserId, roomId)) return BadRequest("Not your room");
        if (!_chatRepository.IsMessageAuthor(messageId, UserId)) return BadRequest("Not your message");
        if (await _chatRepository.DeleteMessage(roomId, messageId) > 0)
        {
            await _chatHubContext.Clients.Group(roomId).MessageDeleted(new MessageId(messageId, roomId));
            return Ok();
        }
        return BadRequest("Message not deleted");
    }

    [HttpDelete("direct-message/{contactId}/{messageId}")]
    public async Task<IActionResult> DeleteDirectMessage(long contactId, long messageId)
    {
        if (!_chatRepository.DirectMessageExists(messageId)) return BadRequest("Message does not exits");
        if (!_chatRepository.IsDirectMessageAuthor(messageId, UserId)) return BadRequest("Not your message");

        var contact = await _userRepository.GetContact(contactId);
        if (contact.Blocked) return BadRequest("User blocked");
        if (!contact.Approved) return BadRequest("Contact was not approved");

        if (await _chatRepository.DeleteDirectMessage(contactId, messageId) > 0)
        {
            await _chatHubContext.Clients.Group(contact.InvitedId).DirectMessageDeleted(new DirectMessageId(messageId, contactId));
            await _chatHubContext.Clients.Group(contact.InviterId).DirectMessageDeleted(new DirectMessageId(messageId, contactId));
            return Ok();
        }
        return BadRequest("Message not deleted");
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

    [HttpPatch("direct-message-viewed/{contactId}/{messageId}")]
    public async Task<IActionResult> DirectMessageViewed(long contactId, long messageId)
    {
        if (!_userRepository.CheckContact(contactId, UserId, out var contact))
        {
            return BadRequest("You cant do that");
        }

        _chatRepository.MarkDirectMessageAsViewed(UserId, contactId, messageId);
        if (await _chatRepository.SaveChanges() > 0)
        {
            var recipientId = contact.InvitedId.Equals(UserId) ? contact.InviterId : contact.InvitedId;
            await _chatHubContext.Clients.Group(recipientId).DirectMessageWasViewed(new DirectMessageViewed(contactId, messageId));
        }

        return Ok();
    }

    [HttpDelete("direct-messages/{contactId}")]
    public async Task<IActionResult> DirectMessages(long contactId)
    {
        if (_userRepository.CheckContactWithMessages(contactId, UserId, out var contact))
        {
            contact.DirectMessages.Clear();
            if (await _chatRepository.SaveChanges() > 0)
            {
                await _chatHubContext.Clients.Group(contact.InviterId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
                await _chatHubContext.Clients.Group(contact.InvitedId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
                return Ok("Chat cleared");
            }
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("direct-messages-viewed")]
    public async Task<IActionResult> DirectMessagesViewed([FromBody] ResourceId resourceId)
    {
        if (!_userRepository.CheckContact(resourceId.Id, UserId, out var contact))
        {
            return BadRequest("You cant do that");
        }

        _chatRepository.MarkDirectMessagesAsViewed(UserId, resourceId.Id);
        var recipientId = contact.InvitedId.Equals(UserId) ? contact.InviterId : contact.InvitedId;
        await _chatHubContext.Clients.Group(recipientId).DirectMessagesWereViewed(new DirectMessagesViewed(resourceId.Id));

        return Ok();
    }

    [HttpPost("add-to-room")]
    public async Task<IActionResult> AddToRoom([FromBody] RoomMemberUpdateRequest roomMemberUpdate)
    {
        var user = await _userManager.FindByNameAsync(roomMemberUpdate.UserName);
        if (!_userRepository.ContactExists(roomMemberUpdate.connectionId, UserId, user.Id)) return BadRequest("This is not your friend");

        var contact = await _userRepository.GetContact(roomMemberUpdate.connectionId);
        if (contact.Blocked) return BadRequest("User blocked");
        if (!contact.Approved) return BadRequest("Contact was not approved");

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
            return Ok();
        }

        return BadRequest();
    }

    private async Task<List<RoomDto>> PrepareRoomDtos(List<Room> rooms)
    {
        var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var iv = Array.ConvertAll(parts, byte.Parse);

        var decryptionErrors = new List<long>();
        foreach (var room in rooms)
        {
            var e2eKey = ExtractE2ECookieDataForRoom(room.Id);
            if (e2eKey is null && room.EncryptedByUser)
            {
                room.Messages.Clear();
            }

            foreach (var message in room.Messages)
            {
                if (room.EncryptedByUser)
                {
                    if (_cryptographyService.Decrypt(message.Text, e2eKey.Key, out var result))
                    {
                        message.Text = result;
                    }
                    else
                    {
                        message.Text = result;
                        decryptionErrors.Add(message.Id);
                    }
                }
                else
                {
                    message.Text = _cryptographyService.Decrypt(message.Text, room.RoomKey, iv);
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
                if (decryptionErrors.Any(de => de == message.Id))
                {
                    message.Error = true;
                }
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

        return roomDtos;
    }

    private async Task<RoomDto> PrepareRoomDto(Room room)
    {
        var parts = _cryptoSettings.CurrentValue.DefaultIV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var iv = Array.ConvertAll(parts, byte.Parse);

        var decryptionErrors = new List<long>();

        var e2eKey = ExtractE2ECookieDataForRoom(room.Id);
        if (e2eKey is null && room.EncryptedByUser)
        {
            room.Messages.Clear();
        }

        foreach (var message in room.Messages)
        {
            if (room.EncryptedByUser)
            {
                if (_cryptographyService.Decrypt(message.Text, e2eKey.Key, out var result))
                {
                    message.Text = result;
                }
                else
                {
                    message.Text = result;
                    decryptionErrors.Add(message.Id);
                }
            }
            else
            {
                message.Text = _cryptographyService.Decrypt(message.Text, room.RoomKey, iv);
            }

            foreach (var viewer in message.MessageViewers)
            {
                viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
            }
        }

        var roomDto = _mapper.Map<RoomDto>(room);
        foreach (var message in roomDto.Messages)
        {
            if (decryptionErrors.Any(de => de == message.Id))
            {
                message.Error = true;
            }
            foreach (var viewer in message.MessageViewers)
            {
                if (viewer.User.Equals(UserName))
                {
                    message.WasViewed = true;
                    continue;
                }
            }
        }

        return roomDto;
    }

    private List<ContactDto> PrepareContactDtos(List<Contact> contacts)
    {
        var decryptionErrors = new List<long>();
        foreach (var contact in contacts)
        {
            var e2eKey = ExtractE2ECookieDataForContact(contact.Id);
            foreach (var message in contact.DirectMessages)
            {

                if (contact.EncryptedByUser)
                {
                    if (e2eKey is null)
                    {
                        message.Text = "Key is missing";
                        continue;
                    }
                    if (_cryptographyService.Decrypt(message.Text, e2eKey.Key, out var result))
                    {
                        message.Text = result;
                    }
                    else
                    {
                        message.Text = result;
                        decryptionErrors.Add(message.Id);
                    }
                }
                else
                {
                    if (_cryptographyService.Decrypt(message.Text, contact.ContactKey, out var result))
                    {
                        message.Text = result;
                    }
                    else
                    {
                        message.Text = result;
                        decryptionErrors.Add(message.Id);
                    }
                }
            }
        }

        var dtos = _mapper.Map<List<ContactDto>>(contacts);
        foreach (var contact in dtos)
        {
            foreach (var message in contact.DirectMessages)
            {
                if (decryptionErrors.Any(de => de == message.Id))
                {
                    message.Error = true;
                }
            }
        }
        return dtos;
    }

    private ContactDto PrepareContactDto(Contact contact)
    {
        var decryptionErrors = new List<long>();

        var e2eKey = ExtractE2ECookieDataForContact(contact.Id);
        foreach (var message in contact.DirectMessages)
        {

            if (contact.EncryptedByUser)
            {
                if (_cryptographyService.Decrypt(message.Text, e2eKey.Key, out var result))
                {
                    message.Text = result;
                }
                else
                {
                    message.Text = result;
                    decryptionErrors.Add(message.Id);
                }
            }
            else
            {
                if (_cryptographyService.Decrypt(message.Text, contact.ContactKey, out var result))
                {
                    message.Text = result;
                }
                else
                {
                    message.Text = result;
                    decryptionErrors.Add(message.Id);
                }
            }
        }

        var dto = _mapper.Map<ContactDto>(contact);
        foreach (var message in dto.DirectMessages)
        {
            if (decryptionErrors.Any(de => de == message.Id))
            {
                message.Error = true;
            }
        }
        return dto;
    }
}
