using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.DbContexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Building> Buildings { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setup table names
        modelBuilder.Entity<Building>().ToTable("Buildings");
        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<Tracker>().ToTable("Trackers");

        // Setup relationships
        modelBuilder.Entity<Building>()
            .HasMany(e => e.Rooms)
            .WithOne(e => e.Building)
            .HasForeignKey(e => e.BuildingId)
            .HasPrincipalKey(e => e.BuildingId);

        // Setup relationships
        modelBuilder.Entity<Room>()
            .HasMany(e => e.Trackers)
            .WithOne(e => e.Room)
            .HasForeignKey(e => e.RoomId)
            .HasPrincipalKey(e => e.RoomId);
    }
}
