using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface ITrackersRepository
{
    Task AddTracker(Tracker tracker);
    Task<IEnumerable<Tracker>> GetTrackersAsync();
}
