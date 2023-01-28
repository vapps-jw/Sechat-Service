using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;

namespace Sechat.Data.Repositories;

public class UserRepository : RepositoryBase<SechatContext>
{
    public UserRepository(SechatContext context) : base(context)
    {
    }

    // Connections

    public bool IsConnected(string user, string otherUser)
    {
        var connection = _context.UserConnections
            .FirstOrDefault(c => (c.InvitedId.Equals(user) || c.InviterId.Equals(user)) && (c.InvitedId.Equals(otherUser) || c.InviterId.Equals(otherUser)));

        return connection is not null && connection.Approved;
    }

    public bool ConnectionExists(string userOne, string userTwo) =>
        _context.UserConnections.Any(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

    public UserConnection GetConnection(string userOne, string userTwo) =>
        _context.UserConnections.FirstOrDefault(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

    public void DeleteConnection(long id)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc => uc.Id == id);
        if (connection is not null)
        {
            _ = _context.UserConnections.Remove(connection);
        }
    }

    public long GetConnectionId(string userOne, string userTwo)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

        return connection is not null ? connection.Id : 0;
    }

    public Task<List<UserConnection>> GetConnections(string userId) =>
        _context.UserConnections
            .Where(uc => uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId))
            .ToListAsync();

    public void BlockConnection(long connectionId, string blockedBy)
    {

    }

    public void UnBlockConnection(long connectionId, string blockedBy)
    {

    }

    public void ApproveConnection(long connectionId, string blockedBy)
    {

    }

    public Task<UserConnection> GetConnection(long connectionId) =>
        _context.UserConnections.FirstOrDefaultAsync(uc => uc.Id == connectionId);

    public UserConnection CreateConnection(string inviterId, string inviterName, string invitedId, string invitedName)
    {
        var newConnection = new UserConnection()
        {
            InvitedId = invitedId,
            InvitedName = invitedName,
            InviterId = inviterId,
            InviterName = inviterName
        };
        _ = _context.UserConnections.Add(newConnection);
        return newConnection;
    }

    // Profile

    public UserProfile GetUserProfile(string id) => _context.UserProfiles
        .FirstOrDefault(p => p.Id.Equals(id));

    public void CreateUserProfile(string id, string userName) => _context
        .Add(new UserProfile() { Id = id, UserName = userName });

    public void DeleteUserProfile(string id)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(id));
        if (profile is not null)
        {
            _ = _context.UserProfiles.Remove(profile);
        }

        // todo: delete connections and do cleanup
    }

    public bool ProfileExists(string id) => _context.UserProfiles
        .Any(p => p.Id.Equals(id));

    public int CountUserProfiles() => _context.UserProfiles.Count();

    // Keys

    public List<Key> GetUserKeys(string userId) => _context.Keys
        .Where(k => k.UserProfileId.Equals(userId))
        .ToList();

    public string GetUserKey(string userId, KeyType keyType) => _context.Keys
        .Where(k => k.UserProfileId.Equals(userId) && k.Type == keyType)
        .Select(k => k.Value)
        .FirstOrDefault();

    public void UpdateEmailConfirmationKey(string userId, string key)
    {
        var userProfile = _context.UserProfiles
            .Include(p => p.Keys)
            .FirstOrDefault(p => p.Id == userId);

        _ = userProfile.Keys.RemoveAll(k => k.Type == KeyType.EmailUpdate);
        userProfile.Keys.Add(new Key() { Type = KeyType.EmailUpdate, Value = key });
    }
}

