using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Room;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class RoomControllerTests
{
    private MockRoomRepository _roomRepository;
    private RoomController _roomController;

    [SetUp]
    public void Setup()
    {
        _roomRepository = new MockRoomRepository();
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

        var removeRoomResponse = await _roomController.RemoveRoom(addRoomResponse.RoomId, default);
        Assert.That(removeRoomResponse.Success, Is.True);

        var rooms = await _roomController.GetRooms(default);
        Assert.That(rooms, Is.Empty);

        var removeRoomResponse2 = await _roomController.RemoveRoom(addRoomResponse.RoomId, default);
        Assert.That(removeRoomResponse2.Success, Is.False);
    }
}
