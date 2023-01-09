using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models;

namespace Sechat.Data;

public class SechatContext : IdentityDbContext, IDataProtectionKeyContext
{
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<Feature> Features { get; set; } = null!;

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public SechatContext(DbContextOptions<SechatContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Features)
            .WithMany(x => x.UserProfiles);

        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Rooms)
            .WithMany(x => x.Members);

        _ = modelBuilder.Entity<UserProfile>()
            .HasMany(x => x.Tokens)
            .WithOne(x => x.UserProfile)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<Room>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Room)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
