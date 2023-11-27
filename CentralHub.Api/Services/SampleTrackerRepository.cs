using System.Collections.Immutable;
using CentralHub.Api.Dtos;

namespace CentralHub.Api.Services;

public sealed class SampleTrackerRepository : ITrackerRepository
{
    private static readonly Dictionary<int, TrackerDto> Trackers = new Dictionary<int, TrackerDto>();
    private static int _nextId;

    private static readonly List<UnregisteredTrackerDto> UnregisteredTrackers = new List<UnregisteredTrackerDto>();


    private static readonly object LockObject = new object();

    static SampleTrackerRepository()
    {
        var sampleRoomRepository = new SampleRoomRepository();
        var sampleTrackerRepository = new SampleTrackerRepository();

        var room1 = sampleRoomRepository.GetRoomByIdAsync(0, default).GetAwaiter().GetResult()!;
        var room2 = sampleRoomRepository.GetRoomByIdAsync(1, default).GetAwaiter().GetResult()!;

        var tracker1 = new TrackerDto()
        {
            WifiMacAddress = "AA:BB:CC:DD:EE:FF",
            BluetoothMacAddress = "FF:EE:DD:CC:BB:AA",
            Name = "Sample Tracker 1",
            Description = "Belongs to Sample Room 1",
            RoomDtoId = room1.RoomDtoId,
            RoomDto = room1,
            TrackerDtoId = -1,
        };

        var tracker2 = new TrackerDto()
        {
            WifiMacAddress = "00:11:22:33:44:55",
            BluetoothMacAddress = "55:44:33:22:11:00",
            Name = "Sample Tracker 2",
            Description = "Belongs to Sample Room 1",
            RoomDtoId = room1.RoomDtoId,
            RoomDto = room1,
            TrackerDtoId = -1,
        };


        var tracker3 = new TrackerDto()
        {
            WifiMacAddress = "00:22:44:66:88:00",
            BluetoothMacAddress = "00:88:66:44:22:00",
            Name = "Sample Tracker 3",
            Description = "Belongs to Sample Room 2",
            RoomDtoId = room2.RoomDtoId,
            RoomDto = room2,
            TrackerDtoId = -1,
        };


        sampleTrackerRepository.AddTrackerAsync(tracker1, default).GetAwaiter().GetResult();
        sampleTrackerRepository.AddTrackerAsync(tracker2, default).GetAwaiter().GetResult();
        sampleTrackerRepository.AddTrackerAsync(tracker3, default).GetAwaiter().GetResult();


        sampleTrackerRepository.AddUnregisteredTracker("BB:AA:DD:CC:FF:EE", "EE:FF:CC:DD:AA:BB", default).GetAwaiter().GetResult();
    }

    public Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            trackerDto.TrackerDtoId = _nextId++;
            Trackers.Add(trackerDto.TrackerDtoId, trackerDto);

            var possibleUnregisteredTracker = UnregisteredTrackers.SingleOrDefault(t =>
                t.WifiMacAddress == trackerDto.WifiMacAddress &&
                t.BluetoothMacAddress == trackerDto.BluetoothMacAddress);

            if (possibleUnregisteredTracker != null)
            {
                UnregisteredTrackers.Remove(possibleUnregisteredTracker);
            }
        }

        return Task.FromResult(trackerDto.TrackerDtoId);
    }

    public Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        // Nothing needs to be done.

        return Task.CompletedTask;
    }

    public Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            Trackers.Remove(trackerDto.TrackerDtoId);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TrackerDto>?> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            var trackers = Trackers.Values.Where(t => t.RoomDtoId == roomId).ToImmutableArray();
            return Task.FromResult<IEnumerable<TrackerDto>?>(trackers);
        }
    }

    public Task<TrackerDto?> GetTrackerAsync(int id, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            if (Trackers.TryGetValue(id, out var trackerDto))
            {
                return Task.FromResult<TrackerDto?>(trackerDto);
            }
        }

        return Task.FromResult<TrackerDto?>(null);
    }

    public Task<TrackerDto?> GetTrackerByMacAddresses(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            var possibleTracker = Trackers.Values.SingleOrDefault(t => t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress);
            return Task.FromResult(possibleTracker);
        }
    }

    public Task<IEnumerable<UnregisteredTrackerDto>> GetUnregisteredTrackers(CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            return Task.FromResult<IEnumerable<UnregisteredTrackerDto>>(UnregisteredTrackers.ToImmutableArray());
        }
    }

    public Task AddUnregisteredTracker(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            UnregisteredTrackers.Add(
                new UnregisteredTrackerDto()
                {
                    WifiMacAddress = wifiMacAddress,
                    BluetoothMacAddress = bluetoothMacAddress
                });
        }

        return Task.CompletedTask;
    }
}