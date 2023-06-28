using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.QueryModels;
using Sechat.Domain.CustomExceptions;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context) { }

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
            Text = messageText,
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

    public bool RoomEncryptedByUser(string roomId) => _context.Rooms.Where(r => r.Id.Equals(roomId)).Select(r => r.EncryptedByUser).FirstOrDefault();

    public byte[] GetRoomKey(string roomId) => _context.Rooms.Where(r => r.Id.Equals(roomId)).Select(r => r.RoomKey).FirstOrDefault();

    public Room CreateRoom(string roomName, string creatorUserId, string creatorName, byte[] roomKey, bool encryptedByUser)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorUserId));
        if (profile == null) return null;

        var newRoom = new Room() { Id = Guid.NewGuid().ToString(), RoomKey = roomKey, CreatorId = creatorUserId, Name = roomName, CreatorName = creatorName, EncryptedByUser = encryptedByUser };
        newRoom.Members.Add(profile);

        _ = _context.Rooms.Add(newRoom);
        return newRoom;
    }

    public string GetRoomName(string roomId) => _context.Rooms.Where(r => r.Id.Equals(roomId)).Select(r => r.Name).FirstOrDefault();

    public List<string> GetRoomMembersIds(string roomId) => _context.Rooms.Include(r => r.Members).FirstOrDefault(r => r.Id.Equals(roomId))?.Members.Select(m => m.Id).ToList();

    public async Task<List<Room>> GetRoomsWithMessages(string memberUserId)
    {
        var res = await _context.Rooms
        .Include(r => r.Messages)
            .ThenInclude(m => m.MessageViewers)
        .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId))).Select(r => new Room()
        {
            EncryptedByUser = r.EncryptedByUser,
            RoomKey = r.RoomKey,
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages.OrderBy(m => m.Id).ToList(),
            Name = r.Name
        }).ToListAsync();
        return res;
    }

    public async Task<Room> GetRoomWithMessages(string roomId)
    {
        var res = await _context.Rooms
        .Where(r => r.Id.Equals(roomId))
        .Include(r => r.Messages)
            .ThenInclude(m => m.MessageViewers)
        .Select(r => new Room()
        {
            EncryptedByUser = r.EncryptedByUser,
            RoomKey = r.RoomKey,
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages.OrderBy(m => m.Id).ToList(),
            Name = r.Name
        }).FirstOrDefaultAsync();
        return res;
    }

    public async Task<List<Room>> GetRoomsWithMessages(string memberUserId, List<GetRoomUpdate> getRoomUpdates)
    {
        var rooms = await _context.Rooms
            .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId)))
            .Select(r => new Room()
            {
                EncryptedByUser = r.EncryptedByUser,
                Name = r.Name,
                RoomKey = r.RoomKey,
                LastActivity = r.LastActivity,
                Created = r.Created,
                CreatorName = r.CreatorName,
                Id = r.Id,
                Members = r.Members
            }).ToListAsync();

        foreach (var room in rooms)
        {
            var updateData = getRoomUpdates.FirstOrDefault(ru => ru.RoomId.Equals(room.Id));
            if (updateData is not null)
            {
                room.Messages = _context.Messages
                    .Where(m => m.RoomId.Equals(room.Id))
                    .Include(m => m.MessageViewers)
                    .Where(m => m.Id > updateData.LastMessage)
                    .OrderBy(m => m.Id)
                    .ToList();
                continue;
            }

            room.Messages = _context.Messages
                .Where(m => m.RoomId.Equals(room.Id))
                .Include(m => m.MessageViewers)
                .OrderBy(m => m.Id)
                .ToList();
        }
        return rooms;
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

