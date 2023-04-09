using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;

namespace Sechat.Data;

public class SechatContext : IdentityDbContext, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<UserConnection> UserConnections { get; set; }
    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }

    public SechatContext(DbContextOptions<SechatContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        _ = modelBuilder.Entity<Message>()
              .HasIndex(c => c.Created);

        _ = modelBuilder.Entity<UserConnection>()
            .HasIndex(c => c.InviterId);
        _ = modelBuilder.Entity<UserConnection>()
            .HasIndex(c => c.InvitedId);

        _ = modelBuilder.Entity<Room>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Room)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
