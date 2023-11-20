using System.ComponentModel;
using System.Diagnostics.Metrics;
using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests;
using CentralHub.Api.Model.Requests.Tracker;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class TrackersControllerTests
{
    private TrackerRepository _trackerRepository;
    private TrackerController _trackerController;

    [SetUp]
    public void Setup()
    {
        var roomDto = new RoomDto()
        {
            Name = "Test Room",
            Description = "Test Room",
        };
        roomDto.RoomDtoId = 1;

        _trackerRepository = new TrackerRepository(roomDto);
        _trackerController = new TrackerController(new RoomRepository(roomDto), _trackerRepository);
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
        await _trackerController.RemoveTracker(trackerId.Value, default);

        var trackers = await _trackerController.GetTrackers(room.RoomDtoId, default);
        Assert.That(trackers.Success, Is.True);
        Assert.That(trackers.Trackers, Is.Empty);
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

    private sealed class RoomRepository : IRoomRepository
    {
        public RoomDto RoomDto { get; }

        public RoomRepository(RoomDto roomDto)
        {
            RoomDto = roomDto;
        }

        public Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new[] { RoomDto }.AsEnumerable());
        }

        public Task<RoomDto> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (id != RoomDto.RoomDtoId)
            {
                return Task.FromResult<RoomDto>(null);
            }

            return Task.FromResult(RoomDto);
        }
    }

    private sealed class TrackerRepository : ITrackerRepository
    {
        private int _nextId = 0;

        public RoomDto RoomDto { get; }

        public TrackerRepository(RoomDto roomDto)
        {
            RoomDto = roomDto;
        }

        public Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
        {
            if (trackerDto.RoomDtoId != RoomDto.RoomDtoId)
            {
                throw new KeyNotFoundException("RoomDto not found");
            }

            trackerDto.TrackerDtoId = _nextId;
            trackerDto.RoomDto = RoomDto;

            _nextId++;

            RoomDto.Trackers.Add(trackerDto);

            return new ValueTask<int>(trackerDto.TrackerDtoId).AsTask();
        }

        public Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
        {
            if (trackerDto.RoomDtoId != RoomDto.RoomDtoId)
            {
                throw new KeyNotFoundException("RoomDto not found");
            }

            RoomDto.Trackers.Remove(trackerDto);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<TrackerDto>> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken)
        {
            if (roomId != RoomDto.RoomDtoId)
            {
                return Task.FromResult<IEnumerable<TrackerDto>>(null);
            }

            return Task.FromResult(RoomDto.Trackers.AsEnumerable());
        }

        public Task<TrackerDto> GetTrackerAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(RoomDto.Trackers.SingleOrDefault(t => t.TrackerDtoId == id));
        }

        public Task<TrackerDto> GetTrackerByMacAddresses(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
        {
            return Task.FromResult(RoomDto.Trackers.SingleOrDefault(t =>
                t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress));
        }
    }
}
