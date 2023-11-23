using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.CalendarModels;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.GlobalModels;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Models.VideoCalls;

namespace Sechat.Data;

public class SechatContext : IdentityDbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
    public DbSet<MessageViewer> MessageViewers { get; set; }
    public DbSet<CallLog> CallLogs { get; set; }
    public DbSet<DirectMessage> DirectMessages { get; set; }
    public DbSet<Blacklisted> Blacklist { get; set; }
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<GlobalSetting> GlobalSettings { get; set; }

    public SechatContext(DbContextOptions<SechatContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<CallLog>()
             .HasIndex(c => c.CalleeId);

        _ = modelBuilder.Entity<Message>()
            .HasIndex(c => c.Created);
        _ = modelBuilder.Entity<Message>()
            .HasMany(x => x.MessageViewers)
            .WithOne(x => x.Message)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<DirectMessage>()
            .HasIndex(c => c.Created);

        _ = modelBuilder.Entity<Feature>()
            .HasIndex(c => c.Name);

        _ = modelBuilder.Entity<MessageViewer>()
            .HasIndex(c => c.UserId);

        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.NotificationSubscriptions)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Features)
            .WithMany(x => x.UserProfiles);
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Rooms)
            .WithMany(x => x.Members);
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Keys)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.CallLogs)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Blacklist)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<UserProfile>()
            .HasOne(x => x.Calendar)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<UserProfile>()
            .HasIndex(c => c.UserName);

        _ = modelBuilder.Entity<Calendar>()
            .HasMany(x => x.CalendarEvents)
            .WithOne(x => x.Calendar)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<CalendarEvent>()
            .HasMany(x => x.Reminders)
            .WithOne(x => x.CalendarEvent)
            .OnDelete(DeleteBehavior.Cascade);
        _ = modelBuilder.Entity<Reminder>()
            .HasIndex(c => c.Remind);
        _ = modelBuilder.Entity<Reminder>()
            .HasIndex(c => c.Reminded);

        _ = modelBuilder.Entity<Contact>()
            .HasIndex(c => c.InviterId);
        _ = modelBuilder.Entity<Contact>()
            .HasIndex(c => c.InvitedId);
        _ = modelBuilder.Entity<Contact>()
            .HasMany(x => x.DirectMessages)
            .WithOne(x => x.Contact)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<Room>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Room)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
