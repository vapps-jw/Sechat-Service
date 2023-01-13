using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context)
    {

    }

    // Rooms

    public Room CreateRoom(string roomName, string creatorUserId, string roomKey)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorUserId));
        if (profile == null) return null;

        var newRoom = new Room() { RoomKey = roomKey, CreatorId = creatorUserId, Name = roomName };
        newRoom.Members.Add(profile);

        _ = _context.Rooms.Add(newRoom);
        return newRoom;
    }

    public void RemoveRoom(long roomId) => _context.Rooms.Remove(_context.Rooms.FirstOrDefault(p => p.Id == roomId));

    public Room GetRoom(long roomId) => _context.Rooms
    .Where(r => r.Id == roomId).Select(r => new Room()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members,
        Messages = r.Messages,
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
        Messages = r.Messages,
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
        Messages = r.Messages,
        Name = r.Name
    }).ToListAsync();

    public void AddToRoom(long roomId, string inviterUserId, string invitedUserId)
    {
        // todo: check connections
        var room = _context.Rooms
            .Where(r => r.Id == roomId)
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return;
        if (!room.Members.Any(m => m.Id.Equals(inviterUserId))) return;
        var newMemberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(invitedUserId));

        room.Members.Add(newMemberProfile);
    }
}

