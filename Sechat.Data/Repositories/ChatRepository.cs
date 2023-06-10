using Microsoft.EntityFrameworkCore;
using Sechat.Data.DataServices;
using Sechat.Data.Models;
using Sechat.Data.QueryModels;
using Sechat.Domain.CustomExceptions;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    private readonly DataEncryptor _dataEncryptor;

    public ChatRepository(SechatContext context, DataEncryptor dataEncryptor) : base(context) => _dataEncryptor = dataEncryptor;

    // Access

    public bool IsRoomAllowed(string userId, string roomId)
    {
        var members = _context.Rooms.Where(r => r.Id.Equals(roomId)).SelectMany(r => r.Members.Select(rm => rm.Id)).ToList();
        return members.Any(m => m.Equals(userId));
    }

    // Messages

    public Message CreateMessage(string userId, string messageText, string roomId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        if (profile == null) return null;

        var room = _context.Rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        room.LastActivity = DateTime.UtcNow;
        profile.LastActivity = DateTime.UtcNow;

        var newMessage = new Message()
        {
            Text = _dataEncryptor.EncryptString(room.RoomKey, messageText),
            IdSentBy = profile.Id,
            NameSentBy = profile.UserName,
            RoomId = room.Id,
            MessageViewers = new List<MessageViewer>() { new MessageViewer(userId) }
        };
        room.Messages.Add(newMessage);
        return newMessage;
    }

    public void MarkMessagesAsViewed(string userId, string roomId)
    {
        var messages = _context.Messages
            .Where(m => m.RoomId.Equals(roomId) && !m.MessageViewers.Any(mv => mv.UserId.Equals(userId)))
            .ToList();

        messages.ForEach(m => m.MessageViewers.Add(new MessageViewer(userId)));
    }

    public void MarkMessageAsViewed(string userId, long messageId)
    {
        var message = _context.Messages
            .FirstOrDefault(m => m.Id == messageId && !m.MessageViewers.Any(mv => mv.UserId.Equals(userId)));

        message?.MessageViewers.Add(new MessageViewer(userId));
    }

    // Rooms

    public Room CreateRoom(string roomName, string creatorUserId, string creatorName, string roomKey)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorUserId));
        if (profile == null) return null;

        var newRoom = new Room() { Id = Guid.NewGuid().ToString(), RoomKey = roomKey, CreatorId = creatorUserId, Name = roomName, CreatorName = creatorName };
        newRoom.Members.Add(profile);

        _ = _context.Rooms.Add(newRoom);
        return newRoom;
    }

    public string GetRoomName(string roomId) => _context.Rooms.Where(r => r.Id.Equals(roomId)).Select(r => r.Name).FirstOrDefault();

    public List<string> GetRoomMembersIds(string roomId) => _context.Rooms.Include(r => r.Members).FirstOrDefault(r => r.Id.Equals(roomId))?.Members.Select(m => m.Id).ToList();

    public async Task<List<Room>> GetStandardRoomsWithMessages(string memberUserId)
    {
        var res = await _context.Rooms
        .Where(r => !string.IsNullOrEmpty(r.RoomKey))
        .Include(r => r.Messages)
            .ThenInclude(m => m.MessageViewers)
        .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId))).Select(r => new Room()
        {
            RoomKey = r.RoomKey,
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages.OrderBy(m => m.Created).ToList(),
            Name = r.Name
        }).ToListAsync();

        res.ForEach(r => r.Messages.ForEach(m => m.Text = _dataEncryptor.DecryptString(r.RoomKey, m.Text)));
        return res;
    }

    public async Task<List<Room>> GetStandardRoomsWithMessagesUpdate(string memberUserId, List<GetRoomUpdate> getRoomUpdates)
    {
        // todo: test and finish this
        var res = await _context.Rooms
        .Where(r => !r.EncryptedByUser && !string.IsNullOrEmpty(r.RoomKey) && r.Members.Any(m => m.Id.Equals(memberUserId)))
        .Select(r => new Room()
        {
            RoomKey = r.RoomKey,
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages
                .Where(m => !getRoomUpdates.Any(ru => ru.RoomId.Equals(m.RoomId)) || m.Created > getRoomUpdates.FirstOrDefault(ru => ru.RoomId.Equals(m.RoomId)).LastMessage)
                .Select(m => new Message()
                {
                    Id = m.Id,
                    Created = m.Created,
                    IdSentBy = m.IdSentBy,
                    NameSentBy = m.NameSentBy,
                    Text = m.Text,
                    MessageViewers = m.MessageViewers.ToList(),
                }).ToList(),
            Name = r.Name
        }).ToListAsync();

        res.ForEach(r => r.Messages.ForEach(m => m.Text = _dataEncryptor.DecryptString(r.RoomKey, m.Text)));
        return res;
    }

    public Room AddToRoom(string roomId, string userId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return null;
        var newMemberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));

        room.Members.Add(newMemberProfile);
        return room;
    }

    public Room RemoveFromRoom(string roomId, string userId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return null;
        if (room.CreatorId.Equals(userId)) throw new ChatException("Cant remove creator of the room");

        var memberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));

        _ = room.Members.Remove(memberProfile);
        return room;
    }

    public bool IsRoomMember(string userId, string roomId) => _context.Rooms
        .Where(r => r.Id.Equals(roomId))
        .SelectMany(r => r.Members)
        .Any(m => m.Id.Equals(userId));

    public bool IsRoomsMember(string userId, List<string> roomId) =>
        _context.Rooms.Where(r => roomId.Contains(r.Id)).All(r => r.Members.Any(m => m.Id.Equals(userId)));

    public Task<Room> GetRoomWithNewMessages(string roomId, DateTime lastMessage) => _context.Rooms
        .Where(r => roomId.Contains(r.Id))
        .Include(r => r.Messages.Where(m => m.Created > lastMessage))
        .FirstOrDefaultAsync();

    public Task<int> DeleteRoom(string roomId, string creatorUserId) => _context.Rooms
        .Where(r => r.Id.Equals(roomId) && r.CreatorId.Equals(creatorUserId))
        .ExecuteDeleteAsync();
}

