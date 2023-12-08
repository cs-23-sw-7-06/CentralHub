using System.Diagnostics.Contracts;
using CentralHub.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.DbContexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RoomDto> Rooms { get; set; } = null!;
    public DbSet<AggregatedMeasurementDto> AggregatedMeasurements { get; set; } = null!;

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
