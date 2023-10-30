using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.DbContexts;

public class DevicesContext : DbContext
{
    public DbSet<Device> Devices { get; set; }

    public DevicesContext(DbContextOptions<DevicesContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>().ToTable("Devices");
    }
}