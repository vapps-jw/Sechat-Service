using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;
using Sechat.Data.Projections;

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

    public bool ConnectionExists(long connectionId, string userOne, string userTwo)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc => uc.Id == connectionId);
        return connection is not null
            && (connection.InvitedId.Equals(userOne) || connection.InviterId.Equals(userOne))
            && (connection.InvitedId.Equals(userTwo) || connection.InviterId.Equals(userTwo));
    }

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

    public UserConnection BlockConnection(long connectionId, string blockedById, string blockedByName)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || connection.Blocked) return null;

        connection.Blocked = true;
        connection.BlockedById = blockedById;
        connection.BlockedByName = blockedByName;

        return connection;
    }

    public UserConnection AllowConnection(long connectionId, string userId)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || !connection.Blocked || !connection.BlockedById.Equals(userId)) return null;

        connection.Blocked = false;
        connection.BlockedById = string.Empty;
        connection.BlockedByName = string.Empty;

        return connection;
    }

    public UserConnection ApproveConnection(long connectionId, string approverId)
    {
        var connection = _context.UserConnections.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || connection.Approved || !connection.InvitedId.Equals(approverId)) return null;
        connection.Approved = true;

        return connection;
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

    public async Task<ProfileDeleteResult> DeleteUserProfile(string id)
    {
        var deletedRooms = new List<string>();
        var deletedConnections = new List<long>();

        var profile = _context.UserProfiles
            .Include(r => r.Rooms)
            .FirstOrDefault(p => p.Id.Equals(id));

        var ownedRooms = profile.Rooms
            .Where(r => r.CreatorId.Equals(profile.Id))
            .ToList();

        var memberRooms = profile.Rooms
            .Where(r => !r.CreatorId.Equals(profile.Id))
            .ToList();

        var connections = await GetConnections(profile.Id);

        if (profile is not null)
        {
            _ = _context.UserProfiles.Remove(profile);
        }

        _context.Rooms.RemoveRange(ownedRooms);
        _context.UserConnections.RemoveRange(connections);

        return new ProfileDeleteResult(
            ownedRooms.Select(r => r.Id).ToList(),
            memberRooms.Select(r => r.Id).ToList(),
            connections);
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

    // Notifications

    public void AddPushNotificationSubscription(NotificationSubscription notificationSubscription) =>
        _context.NotificationSubscriptions.Add(notificationSubscription);

    public void RemovePushNotificationSubscriptions(string userId) =>
        _context.NotificationSubscriptions.RemoveRange(_context.NotificationSubscriptions.Where(s => s.UserProfileId.Equals(userId)));

    public List<NotificationSubscription> GetSubscriptions(string userId) =>
        _context.NotificationSubscriptions.Where(s => s.UserProfileId.Equals(userId)).ToList();

}

