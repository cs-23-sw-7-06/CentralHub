using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Tracker;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public partial class TrackersControllerTests
{
    private MockTrackerRepository _trackerRepository;
    private TrackerController _trackerController;

    [SetUp]
    public void Setup()
    {
        var roomDto = new RoomDto()
        {
            Name = "Test Room",
            Description = "Test Room",
            Capacity = 10,
            NeighbouringRooms = [],
        };

        var roomRepository = new MockRoomRepository();
        roomRepository.AddRoomAsync(roomDto, default).GetAwaiter().GetResult();

        _trackerRepository = new MockTrackerRepository(roomDto);
        _trackerController = new TrackerController(roomRepository, _trackerRepository);
    }

    [Test]
    public async Task TestEmptyByDefault()
    {
        var getTrackersResponse = await _trackerController.GetTrackers(_trackerRepository.RoomDto.RoomDtoId, default);
        Assert.That(getTrackersResponse.Success, Is.True);
        Assert.That(getTrackersResponse.Trackers, Is.Empty);
    }

    [Test]
    public async Task TestAddTracker()
    {
        var room = _trackerRepository.RoomDto;
        var tracker = new AddTrackerRequest(room.RoomDtoId, "Test TrackerDto", "0.1.95", "AA:BB:CC:DD:EE:FF", "00:11:22:33:44:55");
        var addTrackerResponse = await _trackerController.AddTracker(tracker, default);

        // First element is 0
        Assert.That(addTrackerResponse.TrackerId, Is.EqualTo(0));

        var trackers = await _trackerController.GetTrackers(room.RoomDtoId, default);

        Assert.That(trackers.Success, Is.True);
        Assert.That(trackers.Trackers, Is.Not.Null);
        Assert.That(trackers.Trackers.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TestRemoveTracker()
    {
        var room = _trackerRepository.RoomDto;
        var tracker = new AddTrackerRequest(room.RoomDtoId, "Test TrackerDto", "0.1.95", "AA:BB:CC:DD:EE:FF", "00:11:22:33:44:55");
        var addTrackerResponse = await _trackerController.AddTracker(tracker, default);

        var trackerId = addTrackerResponse.TrackerId;
        Assert.That(trackerId.HasValue, Is.True);
        var removeTrackerResponse = await _trackerController.RemoveTracker(trackerId.Value, default);
        Assert.That(removeTrackerResponse.Success, Is.True);

        var trackers = await _trackerController.GetTrackers(room.RoomDtoId, default);
        Assert.That(trackers.Success, Is.True);
        Assert.That(trackers.Trackers, Is.Empty);

        var removeTrackerResponse2 = await _trackerController.RemoveTracker(trackerId.Value, default);
        Assert.That(removeTrackerResponse2.Success, Is.False);
    }

    [Test]
    public async Task TestUnregisteredTracker()
    {
        var trackerRegistrationInfo = await _trackerController.GetTrackerRegistrationInfo("00:00:00:00:00", "00:00:00:00:00", default);

        Assert.That(trackerRegistrationInfo.Registered, Is.False);
        Assert.That(trackerRegistrationInfo.TrackerId, Is.Null);
    }

    [Test]
    public async Task TestRegisteredTracker()
    {
        var addTrackerRequest = new AddTrackerRequest(_trackerRepository.RoomDto.RoomDtoId, "Test Tracker",
            "Test Tracker", "00:00:00:00:00", "00:00:00:00:00");
        var addTrackerResponse = await _trackerController.AddTracker(addTrackerRequest, default);

        var trackerRegistrationInfo =
            await _trackerController.GetTrackerRegistrationInfo(addTrackerRequest.WifiMacAddress,
                addTrackerRequest.BluetoothMacAddress, default);

        Assert.That(trackerRegistrationInfo.Registered, Is.True);
        Assert.That(trackerRegistrationInfo.TrackerId, Is.EqualTo(addTrackerResponse.TrackerId));
    }
}
