using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context)
    {

    }

    // Access

    public bool IsRoomAllowed(string userId, string roomId)
    {
        var members = _context.Rooms.Where(r => r.Id.Equals(roomId)).SelectMany(r => r.Members.Select(rm => rm.Id)).ToList();
        return members.Any(m => m.Equals(userId));
    }

    // Messages

    public Message CreateMessage(string userId, string messageText, string roomId)
    {
        // todo: add encryption

        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        if (profile == null) return null;

        var room = _context.Rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        room.LastActivity = DateTime.UtcNow;

        var newMessage = new Message()
        {
            Text = messageText,
            IdSentBy = profile.Id,
            NameSentBy = profile.UserName,
            RoomId = room.Id
        };
        room.Messages.Add(newMessage);
        return newMessage;
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

    public Room GetRoom(string roomId) => _context.Rooms
    .Where(r => r.Id.Equals(roomId)).Select(r => new Room()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members,
        Messages = r.Messages.OrderBy(m => m.Created).ToList(),
        Name = r.Name
    }).FirstOrDefault();

    public Task<List<Room>> GetRooms(string memberUserId) => _context.Rooms
    .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId))).Select(r => new Room()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members,
        Messages = r.Messages.OrderBy(m => m.Created).ToList(),
        Name = r.Name
    }).ToListAsync();

    public Task<List<Room>> GetCreatedRooms(string creatorUserId) => _context.Rooms
    .Where(r => r.CreatorId.Equals(creatorUserId)).Select(r => new Room()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members,
        Messages = r.Messages.OrderBy(m => m.Created).ToList(),
        Name = r.Name
    }).ToListAsync();

    public void AddToRoom(string roomId, string userId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return;
        var newMemberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));

        room.Members.Add(newMemberProfile);
    }

    public void DeleteRoom(string roomId, string creatorUserId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId) && r.CreatorId.Equals(creatorUserId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return;
        _ = _context.Rooms.Remove(room);
    }
}

