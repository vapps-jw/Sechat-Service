using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.SechatLinqExtensions;
public static class ContactLinqExtensions
{
    public static IQueryable<T> GetContacts<T>(this IQueryable<T> source, string userName) where T : Contact =>
        source.Where(uc => uc.InvitedName.Equals(userName) || uc.InviterName.Equals(userName));
    public static string GetProfilePicture<T>(this IQueryable<T> source, string userName) where T : UserProfile =>
        source.Where(up => up.UserName.Equals(userName))
            .Select(up => up.ProfilePicture)
            .FirstOrDefault();
}
