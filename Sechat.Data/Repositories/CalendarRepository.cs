using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.CalendarModels;

namespace Sechat.Data.Repositories;
public class CalendarRepository : RepositoryBase<SechatContext>
{
    public CalendarRepository(SechatContext context) : base(context) { }

    public Calendar GetCalendar(string userId) => _context.Calendars
        .Where(p => p.UserProfileId.Equals(userId))
        .Include(c => c.CalendarEvents)
        .FirstOrDefault();

    public void AddEvent(string userId, CalendarEvent calendarEvent)
    {
        var calendar = _context.Calendars
            .Where(p => p.UserProfileId.Equals(userId))
            .FirstOrDefault();
        calendar.CalendarEvents.Add(calendarEvent);
    }

    public bool CalendarExists(string userId) => _context.Calendars
        .Any(p => p.UserProfileId.Equals(userId));

    public void CreateCalendar(string userId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        profile.Calendar = new Calendar() { Id = Guid.NewGuid().ToString() };
    }
}
