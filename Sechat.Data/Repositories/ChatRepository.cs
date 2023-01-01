using Sechat.Data.Models;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context)
    {
    }

    public void CreateRoom(Room room) => _context.Add(room);
}

