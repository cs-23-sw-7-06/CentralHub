using System.ComponentModel.DataAnnotations.Schema;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralHub.Api.DbContexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setup table names
        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<Tracker>().ToTable("Trackers");

        // Setup relationships
        modelBuilder.Entity<Room>()
            .HasMany(e => e.Trackers)
            .WithOne(e => e.Room)
            .HasForeignKey(e => e.RoomId)
            .HasPrincipalKey(e => e.RoomId);


    }
}
