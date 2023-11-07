using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class RoomControllerTests
{
    private RoomRepository _roomRepository;
    private RoomController _roomController;

    [SetUp]
    public void Setup()
    {
        _roomRepository = new RoomRepository();
        _roomController = new RoomController(_roomRepository);
    }

    [Test]
    public async Task TestEmptyByDefault()
    {
        var trackers = await _roomController.GetRooms(default);
        Assert.That(trackers, Is.Empty);
    }

    [Test]
    public async Task TestAddRoom()
    {
        var room = new Room("Test Room", "0.1.95");
        await _roomController.AddRoom(room, default);

        var rooms = await _roomController.GetRooms(default);
        Assert.That(rooms.Single(), Is.EqualTo(room));
    }

    [Test]
    public async Task TestRemoveRoom()
    {
        var room = new Room("Test Room", "0.1.95");
        room.RoomId = 1;
        await _roomController.AddRoom(room, default);

        await _roomController.RemoveRoom(room.RoomId, default);

        var rooms = await _roomController.GetRooms(default);
        Assert.That(rooms, Is.Empty);
    }

    private class RoomRepository : IRoomRepository
    {
        private readonly List<Room> _rooms = new List<Room>();

        public Task AddRoomAsync(Room room, CancellationToken cancellationToken)
        {
            _rooms.Add(room);

            return Task.CompletedTask;
        }

        public Task UpdateRoomAsync(Room room, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoomAsync(Room room, CancellationToken cancellationToken)
        {
            _rooms.Remove(room);

            return Task.CompletedTask;
        }

        public Task AddTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<IEnumerable<Room>>(_rooms).AsTask();
        }

        public Task<Room> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
        {
            return new ValueTask<Room>(_rooms.Single(r => r.RoomId == id)).AsTask();
        }
    }
}