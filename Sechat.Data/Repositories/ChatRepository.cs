using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;
using Sechat.Data.Projections;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context)
    {
    }

    // Rooms

    public void CreateRoom(string roomName, string creatorUserId, string roomKey)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorUserId));
        if (profile == null) return;

        var newRoom = new Room() { RoomKey = roomKey, CreatorId = creatorUserId, Name = roomName };
        newRoom.Members.Add(profile);

        _ = _context.Rooms.Add(newRoom);
    }

    public void RemoveRoom(long roomId) => _context.Rooms.Remove(_context.Rooms.FirstOrDefault(p => p.Id == roomId));

    public RoomProjection GetRoom(long roomId) => _context.Rooms
    .Where(r => r.Id == roomId).Select(r => new RoomProjection()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members.Select(m => m.Id).ToList(),
        Name = r.Name
    }).FirstOrDefault();

    public List<RoomProjection> GetRooms(string memberUserId) => _context.Rooms
    .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId))).Select(r => new RoomProjection()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members.Select(m => m.Id).ToList(),
        Name = r.Name
    }).ToList();

    public List<RoomProjection> GetCreatedRooms(string creatorUserId) => _context.Rooms
    .Where(r => r.CreatorId.Equals(creatorUserId)).Select(r => new RoomProjection()
    {
        LastActivity = r.LastActivity,
        Created = r.Created,
        CreatorId = r.CreatorId,
        Id = r.Id,
        Members = r.Members.Select(m => m.Id).ToList(),
        Name = r.Name
    }).ToList();

    public void InviteToRoom(long roomId, string inviterUserId, string invitedUserId)
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

