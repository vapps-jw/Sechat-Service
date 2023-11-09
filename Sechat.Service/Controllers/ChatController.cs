using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.Messages;
using Sechat.Service.Hubs;
using Sechat.Service.Services.CacheServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class ChatController : SechatControllerBase
{
    private const int _initialMessagesPull = 10;
    private const int _updateMessagesPull = 10;

    private readonly SignalRCache _signalRConnectionsMonitor;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly ChatRepository _chatRepository;

    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;

    public ChatController(
        SignalRCache signalRConnectionsMonitor,
        UserManager<IdentityUser> userManager,
        UserRepository userRepository,
        ChatRepository chatRepository,
        IMapper mapper,
        IHubContext<ChatHub, IChatHub> chatHubContext)
    {
        _signalRConnectionsMonitor = signalRConnectionsMonitor;
        _userManager = userManager;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _mapper = mapper;
        _chatHubContext = chatHubContext;
    }

    // For background load

    [HttpGet("contacts-messages-metadata")]
    public async Task<IActionResult> GetContactsMetadataAsync()
    {
        var contacts = await _userRepository.GetContactsMetadata(UserId, _initialMessagesPull);
        var ids = contacts
            .Select(c => new { id = c.InviterId, name = c.InviterName })
            .Concat(contacts.Select(c => new { id = c.InvitedId, name = c.InvitedName }))
            .Distinct()
            .Where(i => !i.id.Equals(UserId))
            .ToList();

        var pictures = _userRepository.GetProfilePictures(ids.Select(i => i.id).ToList());
        var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

        foreach (var dto in contactDtos)
        {
            if (dto.InviterName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InvitedName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }

            if (dto.InvitedName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InviterName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }
        }

        var connectedContacts = contacts
            .Where(c => c.InvitedId.Equals(UserId) ? _signalRConnectionsMonitor.IsUserOnlineFlag(c.InviterId) : _signalRConnectionsMonitor.IsUserOnlineFlag(c.InvitedId))
            .Select(c => c.Id)
            .ToList();

        foreach (var contactDto in contactDtos)
        {
            contactDto.DirectMessages.ForEach(dm => dm.Loaded = false);
            contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
                AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
        }

        return Ok(contactDtos);
    }

    [HttpGet("contact/{contactId}/{messageId}")]
    public async Task<IActionResult> GetContactMessageAsync(long contactId, long messageId)
    {
        if (!_userRepository.CheckContact(contactId, UserId, out var _))
        {
            return BadRequest();
        }

        var message = await _userRepository.GetContactMessage(messageId);
        var messageDto = _mapper.Map<DirectMessageDto>(message);
        messageDto.Loaded = true;

        return Ok(messageDto);
    }

    [HttpGet("rooms-messages-metadata")]
    public async Task<IActionResult> GetRoomsMetadata()
    {
        var rooms = await _chatRepository.GetRoomsMetadata(UserId, _initialMessagesPull);
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
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                }
            }
        }

        return Ok(roomDtos);
    }

    [HttpGet("room/{roomId}/{messageId}")]
    public async Task<IActionResult> GetRoomMessage(string roomId, long messageId)
    {
        if (!_chatRepository.IsRoomMember(UserId, roomId))
        {
            return BadRequest();
        }

        var message = await _chatRepository.GetRoomMessage(messageId);
        foreach (var viewer in message.MessageViewers)
        {
            viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
        }

        var messageDto = _mapper.Map<MessageDto>(message);
        if (messageDto.MessageViewers.Any(mv => mv.User.Equals(UserName)))
        {
            messageDto.WasViewed = true;
        }
        messageDto.Loaded = true;

        return Ok(messageDto);
    }

    // Default load

    [HttpGet("rooms-initial-load")]
    public async Task<IActionResult> RoomsInitialLoad()
    {
        var rooms = await _chatRepository.GetRoomsWithRecentMessages(UserId, _initialMessagesPull);
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
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                }
                message.Loaded = true;
            }
        }

        return Ok(roomDtos);
    }

    [HttpGet("rooms-update/{lastMessage}")]
    public async Task<IActionResult> RoomsUpdate(long lastMessage)
    {
        var rooms = await _chatRepository.GetRoomsUpdate(UserId, lastMessage);
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
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                }
                message.Loaded = true;
            }
        }

        return Ok(roomDtos);
    }

    [HttpGet("rooms-update-metadata/{lastMessage}")]
    public async Task<IActionResult> RoomsUpdateMetadata(long lastMessage)
    {
        var rooms = await _chatRepository.GetRoomsUpdateMetadata(UserId, lastMessage);
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
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                }
                message.Loaded = false;
            }
        }

        return Ok(roomDtos);
    }

    [HttpGet("room-initial-load/{roomId}")]
    public async Task<IActionResult> RoomInitialLoadAsync(string roomId)
    {
        var room = await _chatRepository.GetRoomWithRecentMessages(roomId, UserId, _initialMessagesPull);
        room.Messages = room.Messages.OrderBy(m => m.Id).ToList();

        foreach (var message in room.Messages)
        {
            foreach (var viewer in message.MessageViewers)
            {
                viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
            }
        }

        var roomDto = _mapper.Map<RoomDto>(room);
        foreach (var message in roomDto.Messages)
        {
            foreach (var viewer in message.MessageViewers)
            {
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                    continue;
                }
            }
        }

        return Ok(roomDto);
    }

    [HttpGet("room/{roomId}/load-more/{lastId}")]
    public async Task<IActionResult> LoadMoreRoomMessages(string roomId, long lastId)
    {
        var messages = await _chatRepository.GetOldMessagesForRoom(roomId, lastId, _updateMessagesPull);
        foreach (var message in messages)
        {
            foreach (var viewer in message.MessageViewers)
            {
                viewer.UserId = (await _userManager.FindByIdAsync(viewer.UserId))?.UserName;
            }
        }

        var dtos = _mapper.Map<List<MessageDto>>(messages);

        foreach (var message in dtos)
        {
            foreach (var viewer in message.MessageViewers)
            {
                if (message.MessageViewers.Any(mv => mv.User.Equals(UserName)))
                {
                    message.WasViewed = true;
                    continue;
                }
            }
        }

        return Ok(dtos);
    }

    [HttpGet("contacts-initial-load")]
    public async Task<IActionResult> ContactsInitialLoad()
    {
        var contacts = await _userRepository.GetContactsWithRecentMessages(UserId, _initialMessagesPull);
        var ids = contacts
            .Select(c => new { id = c.InviterId, name = c.InviterName })
            .Concat(contacts.Select(c => new { id = c.InvitedId, name = c.InvitedName }))
            .Distinct()
            .Where(i => !i.id.Equals(UserId))
            .ToList();

        var pictures = _userRepository.GetProfilePictures(ids.Select(i => i.id).ToList());
        var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

        foreach (var dto in contactDtos)
        {
            if (dto.InviterName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InvitedName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }

            if (dto.InvitedName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InviterName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }
        }

        var connectedContacts = contacts
            .Where(c => c.InvitedId.Equals(UserId) ? _signalRConnectionsMonitor.IsUserOnlineFlag(c.InviterId) : _signalRConnectionsMonitor.IsUserOnlineFlag(c.InvitedId))
            .Select(c => c.Id)
            .ToList();

        foreach (var contactDto in contactDtos)
        {
            contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
                AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
        }

        contactDtos.ForEach(c => c.DirectMessages.ForEach(dm => dm.Loaded = true));

        return Ok(contactDtos);
    }

    [HttpGet("contacts-update/{lastMessage}")]
    public async Task<IActionResult> ContactsUpate(long lastMessage)
    {
        var contacts = await _userRepository.GetContactsUpdate(UserId, lastMessage);
        var ids = contacts
            .Select(c => new { id = c.InviterId, name = c.InviterName })
            .Concat(contacts.Select(c => new { id = c.InvitedId, name = c.InvitedName }))
            .Distinct()
            .Where(i => !i.id.Equals(UserId))
            .ToList();

        var pictures = _userRepository.GetProfilePictures(ids.Select(i => i.id).ToList());
        var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

        foreach (var dto in contactDtos)
        {
            if (dto.InviterName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InvitedName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }

            if (dto.InvitedName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InviterName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }
        }

        var connectedContacts = contacts
            .Where(c => c.InvitedId.Equals(UserId) ? _signalRConnectionsMonitor.IsUserOnlineFlag(c.InviterId) : _signalRConnectionsMonitor.IsUserOnlineFlag(c.InvitedId))
            .Select(c => c.Id)
            .ToList();

        foreach (var contactDto in contactDtos)
        {
            contactDto.DirectMessages.ForEach(dm => dm.Loaded = true);
            contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
                AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
        }

        return Ok(contactDtos);
    }

    [HttpGet("contacts-update-metadata/{lastMessage}")]
    public async Task<IActionResult> ContactsUpateMetadata(long lastMessage)
    {
        var contacts = await _userRepository.GetContactsUpdateMetadata(UserId, lastMessage);
        var ids = contacts
            .Select(c => new { id = c.InviterId, name = c.InviterName })
            .Concat(contacts.Select(c => new { id = c.InvitedId, name = c.InvitedName }))
            .Distinct()
            .Where(i => !i.id.Equals(UserId))
            .ToList();

        var pictures = _userRepository.GetProfilePictures(ids.Select(i => i.id).ToList());
        var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

        foreach (var dto in contactDtos)
        {
            if (dto.InviterName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InvitedName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }

            if (dto.InvitedName.Equals(UserName))
            {
                var imageId = ids.FirstOrDefault(i => i.name.Equals(dto.InviterName));
                dto.ProfileImage = pictures[imageId.id];
                continue;
            }
        }

        var connectedContacts = contacts
            .Where(c => c.InvitedId.Equals(UserId) ? _signalRConnectionsMonitor.IsUserOnlineFlag(c.InviterId) : _signalRConnectionsMonitor.IsUserOnlineFlag(c.InvitedId))
            .Select(c => c.Id)
            .ToList();

        foreach (var contactDto in contactDtos)
        {
            contactDto.DirectMessages.ForEach(dm => dm.Loaded = false);
            contactDto.ContactState = connectedContacts.Contains(contactDto.Id) ?
                AppConstants.ContactState.Online : AppConstants.ContactState.Offline;
        }

        return Ok(contactDtos);
    }

    [HttpGet("contact/{contactId}/load-more/{lastId}")]
    public async Task<IActionResult> LoadMoreContactMessages(long contactId, long lastId)
    {
        var messages = await _userRepository.GetOldMessagesForContact(contactId, lastId, _updateMessagesPull);
        var dtos = _mapper.Map<List<DirectMessageDto>>(messages);

        return Ok(dtos);
    }

    [HttpPost("send-message")]
    [RequestSizeLimit(80_000_000)]
    public async Task<IActionResult> SendMessage(
    [FromServices] Channel<DefaultNotificationDto> channel,
    [FromBody] IncomingMessage incomingMessageDto)
    {
        if (!_chatRepository.IsRoomMember(UserId, incomingMessageDto.RoomId))
        {
            return BadRequest("You dont have access to this room");
        }

        var res = _chatRepository.CreateMessage(UserId, incomingMessageDto.Text, incomingMessageDto.RoomId);
        if (await _chatRepository.SaveChanges() == 0)
        {
            return BadRequest("Message not sent");
        }

        var messageDto = _mapper.Map<MessageDto>(res);

        foreach (var viewer in messageDto.MessageViewers)
        {
            viewer.User = (await _userManager.FindByIdAsync(viewer.User))?.UserName;
        }
        messageDto.Loaded = true;

        var excluded = _signalRConnectionsMonitor.ConnectedUsers[UserId];
        await _chatHubContext.Clients.GroupExcept(incomingMessageDto.RoomId, excluded).MessageIncoming(messageDto);

        var roomMembers = _chatRepository.GetRoomMembersIds(incomingMessageDto.RoomId);
        _ = roomMembers.RemoveAll(m => m.Equals(UserId));
        if (!roomMembers.Any()) return Ok();

        var roomName = _chatRepository.GetRoomName(incomingMessageDto.RoomId);
        foreach (var member in roomMembers)
        {
            await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.IncomingMessage, member, roomName));
        }

        messageDto.Text = string.Empty;
        return Ok(messageDto);
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

    [HttpPatch("rename-room")]
    public async Task<IActionResult> RenameRoom([FromBody] ChatControllerFroms.RenameRoomReuqest requestForm)
    {
        if (!_chatRepository.IsRoomMember(UserId, requestForm.RoomId)) return BadRequest("Not room member");

        var room = _chatRepository.GetRoom(requestForm.RoomId);
        room.Name = requestForm.NewName;
        if (await _chatRepository.SaveChanges() > 0)
        {
            var roomDto = _mapper.Map<RoomDto>(room);
            await _chatHubContext.Clients.Group(requestForm.RoomId).RoomUpdated(roomDto);
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

    // Contacts

    [HttpGet("contact/{contactId}")]
    public async Task<IActionResult> Contact(long contactId)
    {
        if (!_userRepository.CheckContact(contactId, UserId, out _))
        {
            return BadRequest("Contact not allowed");
        }

        var contact = await _userRepository.GetContactWithRecentMessages(contactId, _initialMessagesPull);
        var contactDto = _mapper.Map<ContactDto>(contact);

        return Ok(contactDto);
    }

    [HttpPost("send-direct-message")]
    [RequestSizeLimit(80_000_000)]
    public async Task<IActionResult> SendDirectMessage(
        [FromServices] Channel<DefaultNotificationDto> channel,
        [FromBody] IncomingDirectMessage incomingMessageDto)
    {
        var recipient = await _userManager.FindByNameAsync(incomingMessageDto.Recipient);
        var contact = _userRepository.GetContact(UserId, recipient.Id);

        if (contact.Blocked) return BadRequest("User blocked");
        if (!contact.Approved) return BadRequest("Contact was not approved");

        var res = _chatRepository.CreateDirectMessage(UserId, incomingMessageDto.Text, contact.Id);
        if (await _chatRepository.SaveChanges() == 0)
        {
            return BadRequest("Message not sent");
        }

        var messageDto = _mapper.Map<DirectMessageDto>(res);

        messageDto.Loaded = true;

        await _chatHubContext.Clients.Group(recipient.Id).DirectMessageIncoming(messageDto);
        await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.IncomingDirectMessage, recipient.Id, UserName));

        messageDto.Text = string.Empty;
        return Ok(messageDto);
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
            _ = await _chatRepository.SaveChanges();
            await _chatHubContext.Clients.Group(contact.InviterId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
            await _chatHubContext.Clients.Group(contact.InvitedId).ContactUpdateRequired(new ContactUpdateRequired(contact.Id));
            return Ok("Chat cleared");
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
}

public class ChatControllerFroms
{
    public class RenameRoomReuqest
    {
        public string RoomId { get; set; }
        public string NewName { get; set; }
    }
    public class RenameRoomReuqestValidation : AbstractValidator<RenameRoomReuqest>
    {
        public RenameRoomReuqestValidation()
        {
            _ = RuleFor(x => x.RoomId).NotNull().NotEmpty();
            _ = RuleFor(x => x.NewName).NotNull().NotEmpty();
        }
    }
}

