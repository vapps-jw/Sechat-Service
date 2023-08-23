using Sechat.Data.Models.CalendarModels;

namespace Sechat.Data.Repositories;
public class CalendarRepository : RepositoryBase<SechatContext>
{
    public CalendarRepository(SechatContext context) : base(context) { }

    public bool CalendarExists(string userId) => _context.Calendars
        .Any(p => p.UserProfileId.Equals(userId));

    public void CreateCalendar(string userId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        profile.Calendar = new Calendar() { Id = Guid.NewGuid().ToString() };
    }
}
