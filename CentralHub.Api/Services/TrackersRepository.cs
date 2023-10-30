using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

public class TrackersRepository : ITrackersRepository
{
    private readonly TrackersContext _trackersContext;

    public TrackersRepository(TrackersContext trackersContext)
    {
        _trackersContext = trackersContext;
        _trackersContext.Database.OpenConnection();
        _trackersContext.Database.EnsureCreated();
    }

    public async Task AddTracker(Tracker tracker)
    {
        await _trackersContext.Trackers.AddAsync(tracker);
        await _trackersContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Tracker>> GetTrackersAsync()
    {
        return await _trackersContext.Trackers.ToArrayAsync();
    }
}
