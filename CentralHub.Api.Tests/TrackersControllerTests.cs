using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class TrackersControllerTests
{
    private TrackersController _trackersController;

    [SetUp]
    public void Setup()
    {
        var trackersRepository = new TrackersRepository();
        _trackersController = new TrackersController(trackersRepository);
    }

    [Test]
    public async Task TestEmptyByDefault()
    {
        var trackers = await _trackersController.Get();
        Assert.That(trackers, Is.Empty);
    }

    [Test]
    public async Task TestAddTracker()
    {
        var tracker = new Tracker("Test Tracker", "AA:BB:CC:DD:EE:FF", TrackerType.WiFi);
        await _trackersController.Post(tracker);

        var trackers = await _trackersController.Get();
        Assert.That(trackers.Single(), Is.EqualTo(tracker));
    }

    private class TrackersRepository : ITrackersRepository
    {
        private readonly List<Tracker> _trackers = new List<Tracker>();

        public Task AddTracker(Tracker tracker)
        {
            _trackers.Add(tracker);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Tracker>> GetTrackersAsync()
        {
            return Task.FromResult<IEnumerable<Tracker>>(_trackers);
        }
    }
}
