using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Projections;
using Sechat.Data.QueryModels;

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

    public bool CheckContact(long contactId, string userId, out Contact contact)
    {
        contact = _context.Contacts
            .Where(c => c.Id == contactId && (c.InvitedId.Equals(userId) || c.InviterId.Equals(userId)))
            .FirstOrDefault();

        return contact is not null && !contact.Blocked && contact.Approved;
    }

    public bool CheckContactWithMessages(long contactId, string userId, out Contact contact)
    {
        contact = _context.Contacts
            .Where(c => c.Id == contactId && (c.InvitedId.Equals(userId) || c.InviterId.Equals(userId)))
            .Include(c => c.DirectMessages)
            .FirstOrDefault();

        return contact is not null && !contact.Blocked && contact.Approved;
    }

    public bool CheckContactAndGetContactId(string userName, string contactName, out string contactId)
    {
        contactId = string.Empty;
        var contact = _context.Contacts
            .Where(c => (c.InvitedName.Equals(userName) || c.InviterName.Equals(userName)) &&
                        (c.InvitedName.Equals(contactName) || c.InviterName.Equals(contactName)))
            .FirstOrDefault();
        if (contact is not null && !contact.Blocked && contact.Approved)
        {
            contactId = contact.InvitedName.Equals(userName) ? contact.InviterId : contact.InvitedId;
            return true;
        }
        return false;
    }

    public bool ContactExists(long contactId, string userOne, string userTwo)
    {
        var connection = _context.Contacts.FirstOrDefault(uc => uc.Id == contactId);
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
        var contact = _context.Contacts.FirstOrDefault(uc => uc.Id == id);
        if (contact is not null)
        {
            _ = _context.Contacts.Remove(contact);
        }
    }

    public Task<List<Contact>> GetContacts(string userId) =>
        _context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId))
            .ToListAsync();

    public Task<List<Contact>> GetContactsWithMessages(string userId) =>
        _context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId))
            .Include(c => c.DirectMessages.OrderBy(dm => dm.Id))
            .ToListAsync();

    public Task<Contact> GetContact(long contactId) =>
        _context.Contacts.FirstOrDefaultAsync(c => c.Id == contactId);

    public Task<Contact> GetContactWithMessages(long contactId) =>
        _context.Contacts
            .Include(c => c.DirectMessages.OrderBy(dm => dm.Id))
            .FirstOrDefaultAsync(c => c.Id == contactId);

    public Task<List<Contact>> GetContactsWithMessages(string userId, List<GetContactUpdate> contactsToUpdate) =>
        _context.Contacts
            .Where(uc => contactsToUpdate.Any(ctu => ctu.ContactId == uc.Id) && (uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId)))
            .Include(c => c.DirectMessages.Where(dm => contactsToUpdate.First(c => c.ContactId == c.ContactId).LastMessage < dm.Id))
            .ToListAsync();

    public Task<List<string>> GetAllowedContactsIds(string userId) =>
        _context.Contacts
            .Where(uc => !uc.Blocked && uc.Approved && (uc.InvitedId.Equals(userId) || uc.InviterId.Equals(userId)))
            .Select(uc => uc.InvitedId.Equals(userId) ? uc.InviterId : uc.InvitedId)
            .ToListAsync();

    public Task<List<Contact>> GetAllowedContacts(string userId) =>
        _context.Contacts
            .Where(uc => uc.InvitedId.Equals(userId) || (uc.InviterId.Equals(userId) && !uc.Blocked && uc.Approved))
            .ToListAsync();

    public bool IsContactAllowed(string userId, string contactId) =>
        _context.Contacts
            .Any(uc => ((uc.InvitedId.Equals(userId) && uc.InviterId.Equals(contactId)) ||
                        (uc.InvitedId.Equals(contactId) && uc.InviterId.Equals(userId))) &&
                        !uc.Blocked &&
                        uc.Approved);

    public Contact BlockContact(long connectionId, string blockedById, string blockedByName)
    {
        var contact = _context.Contacts.FirstOrDefault(uc => uc.Id == connectionId);
        if (contact is null || contact.Blocked) return null;

        contact.Blocked = true;
        contact.BlockedById = blockedById;
        contact.BlockedByName = blockedByName;

        return contact;
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

    public Contact CreateContact(string inviterId, string inviterName, string invitedId, string invitedName, string contactKey)
    {
        var newContact = new Contact()
        {
            InvitedId = invitedId,
            InvitedName = invitedName,
            InviterId = inviterId,
            InviterName = inviterName,
        };
        _ = _context.Contacts.Add(newContact);
        return newContact;
    }

    // Profile

    public void UpdateUserActivity(string userId)
    {
        _ = _context.UserProfiles
            .Where(p => p.Id.Equals(userId))
            .ExecuteUpdate(setters => setters
                .SetProperty(m => m.LastActivity, DateTime.UtcNow));
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

    public void UpdatKey(string userId, KeyType keyType, string value)
    {
        var profile = _context.UserProfiles
            .Include(p => p.Keys)
            .FirstOrDefault(p => p.Id.Equals(userId));

        if (profile.Keys.Any())
        {
            var currentKey = profile.Keys.FirstOrDefault(k => k.Type == keyType);
            if (currentKey is not null)
            {
                _ = _context.Keys.Remove(currentKey);
            }
        }
        profile.Keys.Add(new Key() { Type = keyType, Value = value });
    }

    public List<Key> GetUserKeys(string userId) => _context.Keys
        .Where(k => k.UserProfileId.Equals(userId))
        .ToList();

    public bool KeyExists(string userId, KeyType keyType) => _context.Keys
        .Where(k => k.UserProfileId.Equals(userId))
        .Any(k => k.Type == keyType);

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

