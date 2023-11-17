using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Room;
using CentralHub.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentralHub.Api.Tests;

public class RoomControllerTests
{
    private RoomRepository _roomRepository;
    private ILocalizationService _localizationService;
    private RoomController _roomController;

    [SetUp]
    public void Setup()
    {
        _roomRepository = new RoomRepository();
        _localizationService = new LocalizationService();
        _roomController = new RoomController(NullLogger<RoomController>.Instance, _roomRepository, _localizationService);
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
        var room = new AddRoomRequest("Test Room 1", "0.1.95");
        var addRoomResponse = await _roomController.AddRoom(room, default);

        // Should be 0
        Assert.That(addRoomResponse.RoomId, Is.EqualTo(0));
        var rooms = await _roomController.GetRooms(default);
        Assert.That(rooms.Count(), Is.EqualTo(1));

        // Add another and check the id
        var room2 = new AddRoomRequest("Test Room 2", "123");
        var addRoomResponse2 = await _roomController.AddRoom(room2, default);
        Assert.That(addRoomResponse2.RoomId, Is.EqualTo(1));
        var rooms2 = await _roomController.GetRooms(default);
        Assert.That(rooms2.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task TestRemoveRoom()
    {
        var room = new AddRoomRequest("Test Room", "0.1.95");
        var addRoomResponse = await _roomController.AddRoom(room, default);

        await _roomController.RemoveRoom(addRoomResponse.RoomId, default);

        var rooms = await _roomController.GetRooms(default);
        Assert.That(rooms, Is.Empty);
    }

    private class RoomRepository : IRoomRepository
    {
        private readonly List<RoomDto> _rooms = new List<RoomDto>();
        private int _nextId = 0;

        public Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            _rooms.Add(roomDto);
            roomDto.RoomDtoId = _nextId;
            _nextId++;

            return new ValueTask<int>(roomDto.RoomDtoId).AsTask();
        }

        public Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            _rooms.Remove(roomDto);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<IEnumerable<RoomDto>>(_rooms).AsTask();
        }

        public Task<RoomDto> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
        {
            return new ValueTask<RoomDto>(_rooms.Single(r => r.RoomDtoId == id)).AsTask();
        }
    }
}
