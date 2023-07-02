using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Projections;

namespace Sechat.Data.Repositories;

public class UserRepository : RepositoryBase<SechatContext>
{
    public UserRepository(SechatContext context) : base(context)
    {
    }

    // Contacts

    public bool ContactExists(string userOne, string userTwo) =>
        _context.Contacts.Any(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

    public bool ContactExists(long connectionId, string userOne, string userTwo)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == connectionId);
        return connection is not null
            && (connection.InvitedId.Equals(userOne) || connection.InviterId.Equals(userOne))
            && (connection.InvitedId.Equals(userTwo) || connection.InviterId.Equals(userTwo));
    }

    public Contact GetContact(string userOne, string userTwo) =>
        _context.Contacts.FirstOrDefault(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

    public void DeleteContact(long id)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == id);
        if (connection is not null)
        {
            _ = _context.Contacts.Remove(connection);
        }
    }

    public void DeleteContactsFor(string userId) => _context.Contacts.RemoveRange(_context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId)));

    public long GetContactId(string userOne, string userTwo)
    {
        var connection = _context.Contacts.FirstOrDefault(uc =>
            (uc.InvitedId.Equals(userTwo) && uc.InviterId.Equals(userOne)) ||
            (uc.InviterId.Equals(userTwo) && uc.InvitedId.Equals(userOne)));

        return connection is not null ? connection.Id : 0;
    }

    public Task<List<Contact>> GetContacts(string userId) =>
        _context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId))
            .ToListAsync();

    public Task<List<string>> GetAllowedContactsIds(string userId) =>
        _context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || (uc.InviterId.Equals(userId) && !uc.Blocked && uc.Approved))
            .Select(uc => uc.InvitedId.Equals(userId) ? uc.InviterId : uc.InvitedId)
            .ToListAsync();

    public Contact BlockContact(long connectionId, string blockedById, string blockedByName)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || connection.Blocked) return null;

        connection.Blocked = true;
        connection.BlockedById = blockedById;
        connection.BlockedByName = blockedByName;

        return connection;
    }

    public Contact AllowContact(long connectionId, string userId)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || !connection.Blocked || !connection.BlockedById.Equals(userId)) return null;

        connection.Blocked = false;
        connection.BlockedById = string.Empty;
        connection.BlockedByName = string.Empty;

        return connection;
    }

    public Contact ApproveContact(long connectionId, string approverId)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == connectionId);
        if (connection is null || connection.Approved || !connection.InvitedId.Equals(approverId)) return null;
        connection.Approved = true;

        return connection;
    }

    public Task<Contact> GetContact(long connectionId) =>
        _context.Contacts.FirstOrDefaultAsync(uc => uc.Id == connectionId);

    public Contact CreateContact(string inviterId, string inviterName, string invitedId, string invitedName, string contactKey)
    {
        var newConnection = new Contact()
        {
            InvitedId = invitedId,
            InvitedName = invitedName,
            InviterId = inviterId,
            InviterName = inviterName,
            ContactKey = contactKey
        };
        _ = _context.Contacts.Add(newConnection);
        return newConnection;
    }

    // Profile

    public void UpdateUserActivity(string userId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        if (profile == null) return;
        profile.LastActivity = DateTime.UtcNow;
    }

    public UserProfile GetUserProfile(string id) => _context.UserProfiles
        .FirstOrDefault(p => p.Id.Equals(id));

    public void CreateUserProfile(string id, string userName) => _context
        .Add(new UserProfile() { Id = id, UserName = userName, PrivacyPolicyAccepted = true });

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

        var connections = await GetContacts(profile.Id);

        if (profile is not null)
        {
            _ = _context.UserProfiles.Remove(profile);
        }

        _context.Rooms.RemoveRange(ownedRooms);
        _context.Contacts.RemoveRange(connections);

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

    public void AddPushNotificationSubscription(NotificationSubscription notificationSubscription) => _context.NotificationSubscriptions.Add(notificationSubscription);

    public bool AlreadySubscribed(NotificationSubscription notificationSubscription) => _context.NotificationSubscriptions
        .Any(s =>
            s.Endpoint.Equals(notificationSubscription.Endpoint) &&
            s.Auth.Equals(notificationSubscription.Auth) &&
            s.P256dh.Equals(notificationSubscription.P256dh) &&
            s.UserProfileId.Equals(notificationSubscription.UserProfileId));

    public void RemovePushNotificationSubscriptions(string userId) =>
        _context.NotificationSubscriptions.RemoveRange(_context.NotificationSubscriptions.Where(s => s.UserProfileId.Equals(userId)));
    public void RemovePushNotificationSubscription(int subId) =>
        _context.NotificationSubscriptions.Remove(_context.NotificationSubscriptions.FirstOrDefault(s => s.Id == subId));

    public List<NotificationSubscription> GetSubscriptions(string userId) =>
        _context.NotificationSubscriptions.Where(s => s.UserProfileId.Equals(userId)).ToList();

}

