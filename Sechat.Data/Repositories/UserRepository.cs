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
            .FirstOrDefault(c => (c.Invited.Equals(user) || c.Inviter.Equals(user)) && (c.Invited.Equals(otherUser) || c.Inviter.Equals(otherUser)));

        if (connection is null)
        {
            return false;
        }

        if (!connection.Approved)
        {
            return false;
        }

        return true;
    }

    // Profile

    public UserProfile GetUserProfile(string id) => _context.UserProfiles
        .FirstOrDefault(p => p.Id.Equals(id));
    public void CreateUserProfile(string id) => _context
        .Add(new UserProfile() { Id = id });
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
