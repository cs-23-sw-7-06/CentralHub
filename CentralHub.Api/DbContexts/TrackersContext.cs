using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.DbContexts;

public class TrackersContext : DbContext
{
    public DbSet<Tracker> Trackers { get; set; }

    public TrackersContext(DbContextOptions<TrackersContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tracker>().ToTable("Trackers");
    }
}
