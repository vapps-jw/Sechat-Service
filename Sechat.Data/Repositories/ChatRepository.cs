using Sechat.Data.Models;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context)
    {
    }

    public void CreateRoom(string creatorId, string roomKey)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorId));
        var newRoom = new Room() { RoomKey = roomKey };
        newRoom.Members.Add(profile);

        _ = _context.Add(newRoom);
    }
}

