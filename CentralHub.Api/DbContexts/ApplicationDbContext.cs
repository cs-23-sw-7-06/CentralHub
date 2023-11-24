using System.Diagnostics.Contracts;
using CentralHub.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.DbContexts;

public class ApplicationDbContext : DbContext
{
    public DbSet<RoomDto> Rooms { get; set; }
    public DbSet<AggregatedMeasurementDto> AggregatedMeasurements { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setup table names
        modelBuilder.Entity<RoomDto>().ToTable("Rooms");
        modelBuilder.Entity<TrackerDto>().ToTable("Trackers");
        modelBuilder.Entity<AggregatedMeasurementDto>().ToTable("AggregatedMeasurements");

        // Setup relationships
        modelBuilder.Entity<RoomDto>()
            .HasMany(e => e.Trackers)
            .WithOne(e => e.RoomDto)
            .HasForeignKey(e => e.RoomDtoId)
            .IsRequired()
            .HasPrincipalKey(e => e.RoomDtoId);

        modelBuilder.Entity<AggregatedMeasurementDto>()
            .HasOne(e => e.RoomDto)
            .WithMany(e => e.AggregatedMeasurements)
            .HasForeignKey(e => e.RoomDtoId)
            .IsRequired()
            .HasPrincipalKey(e => e.RoomDtoId);
    }
}
