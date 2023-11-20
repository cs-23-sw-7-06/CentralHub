using System.ComponentModel;
using System.Diagnostics.Metrics;
using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class TrackersControllerTests
{
    private RoomRepository _roomRepository;
    private ILocalizationTargetService _localizationTargetService;
    private TrackerController _trackerController;

    [SetUp]
    public void Setup()
    {
        _roomRepository = new RoomRepository();
        _localizationTargetService = new LocalizationTargetService();
        _trackerController = new TrackerController(_roomRepository, _localizationTargetService);
    }

    [Test]
    public async Task TestEmptyByDefault()
    {
        var trackers = await _trackerController.GetTrackers(1, default);
        Assert.That(trackers, Is.Empty);
    }

    [Test]
    public async Task TestAddTracker()
    {
        var room = _roomRepository.Room;
        var tracker = new Tracker("Test Tracker", "0.1.95", "AA:BB:CC:DD:EE:FF", room);
        await _trackerController.AddTracker(tracker, default);

        var trackers = await _trackerController.GetTrackers(room.RoomId, default);
        Assert.That(trackers.Single(), Is.EqualTo(tracker));
    }

    [Test]
    public async Task TestRemoveTracker()
    {
        var room = _roomRepository.Room;
        var tracker = new Tracker("Test Tracker", "0.1.95", "AA:BB:CC:DD:EE:FF", room);
        await _trackerController.AddTracker(tracker, default);

        await _trackerController.RemoveTracker(tracker, default);

        var trackers = await _trackerController.GetTrackers(room.RoomId, default);
        Assert.That(trackers, Is.Empty);
    }
    [Test]
    public void TestAddMeasurements()
    {
        var measurements = new List<Measurement>(){
            new Measurement(Measurement.Protocol.Bluetooth, "11:22:33:44:55:66", 10),
            new Measurement(Measurement.Protocol.Wifi, "aa:bb:cc:dd:ee:ff", 20)};
        _localizationTargetService.AddMeasurements(0, measurements);

        var gotMeasurements = _localizationTargetService.GetMeasurementsForId(0, default);

        Assert.That(measurements.All(gotMeasurements.Contains));
    }
    [Test]
    public async Task TestRemoveMeasurement()
    {
        var measurement = new Measurement(Measurement.Protocol.Wifi, "11:22:33:44:55:66", 1);
        _localizationTargetService.AddMeasurements(0, new List<Measurement>() { measurement });
        Task.Delay(180000); // Sleep for 3 minutes, making sure the 2 minute timeout of adding a measurement is up.
        Assert.That(_localizationTargetService.GetMeasurementsForId(0, default).Count == 0);
    }

    private class RoomRepository : IRoomRepository
    {
        public Room Room { get; }

        private readonly List<Tracker> _trackers = new List<Tracker>();
        public RoomRepository()
        {
            Room = new Room("Test Room", "Test Room");
            Room.RoomId = 1;
        }

        public Task AddRoomAsync(Room room, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRoomAsync(Room room, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoomAsync(Room room, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            if (tracker.RoomId != Room.RoomId)
            {
                throw new KeyNotFoundException("Room not found");
            }

            Room.Trackers.Add(tracker);

            return Task.CompletedTask;
        }

        public Task UpdateTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            if (tracker.RoomId != Room.RoomId)
            {
                throw new KeyNotFoundException("Room not found");
            }

            Room.Trackers.Remove(tracker);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<IEnumerable<Room>>(new[] { Room }).AsTask();
        }

        public Task<Room> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (id != Room.RoomId)
            {
                throw new KeyNotFoundException("Room not found");
            }

            return new ValueTask<Room>(Room).AsTask();
        }

    }
}
