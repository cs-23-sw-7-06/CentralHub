using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class TrackersControllerTests
{
    private RoomRepository _roomRepository;
    private TrackerController _trackerController;

    [SetUp]
    public void Setup()
    {
        _roomRepository = new RoomRepository();
        _trackerController = new TrackerController(_roomRepository);
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

    private class RoomRepository : IRoomRepository
    {
        public Room Room { get; }

        private readonly List<Tracker> _trackers = new List<Tracker>();

        private List<KeyValuePair<int, Measurement>> Measurements = new List<KeyValuePair<int, Measurement>>();

        public RoomRepository()
        {
            Room = new Room("Test Room", "Test Room", _trackers);
            Room.RoomId = 1;
            Measurements.Add(new KeyValuePair<int, Measurement>(1, new Measurement("wifi", "12:34:56:78:90", 1)));
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
            _trackers.Add(tracker);

            return Task.CompletedTask;
        }

        public Task UpdateTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            _trackers.Remove(tracker);

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

        

        public Task AddMeasurementsAsync(MeasurementCollection measurements, CancellationToken token)
        {
            if(Measurements == null) throw new NullReferenceException();
            foreach (var measurement in measurements.Measurements)
            {
                Measurements.Add(new KeyValuePair<int, Measurement>(measurements.TrackerId, measurement));
            }
            return Task.CompletedTask;
        }

        public Task<ICollection<Measurement>> GetMeasurementsAsync(int id, CancellationToken token)
        {
            if(Measurements[0].Value != new Measurement("wifi", "12:34:56:78:90", 1)) throw new InvalidDataException("Measurement list does not contain the expected values");

            return new ValueTask<ICollection<Measurement>>((ICollection<Measurement>)Measurements).AsTask();


        }
    }
}
