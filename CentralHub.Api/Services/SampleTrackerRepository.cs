using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Threading;

namespace CentralHub.Api.Services;

public sealed class SampleTrackerRepository : ITrackerRepository
{
    private sealed class LockedStuff
    {
        public Dictionary<int, TrackerDto> Trackers { get; } = new Dictionary<int, TrackerDto>();
        public int NextId { get; set; }
        public List<UnregisteredTrackerDto> UnregisteredTrackers { get; } = new List<UnregisteredTrackerDto>();
    }

    private static readonly CancellableMutex<LockedStuff> LockedStuffMutex = new CancellableMutex<LockedStuff>(new LockedStuff());

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

    public async Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        await LockedStuffMutex.Lock(stuff =>
        {
            trackerDto.TrackerDtoId = stuff.NextId++;
            stuff.Trackers.Add(trackerDto.TrackerDtoId, trackerDto);

            var possibleUnregisteredTracker = stuff.UnregisteredTrackers.SingleOrDefault(t =>
                t.WifiMacAddress == trackerDto.WifiMacAddress &&
                t.BluetoothMacAddress == trackerDto.BluetoothMacAddress);

            if (possibleUnregisteredTracker != null)
            {
                stuff.UnregisteredTrackers.Remove(possibleUnregisteredTracker);
            }
        }, cancellationToken);

        return trackerDto.TrackerDtoId;
    }

    public Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        // Nothing needs to be done.

        return Task.CompletedTask;
    }

    public async Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        await LockedStuffMutex.Lock(stuff =>
        {
            stuff.Trackers.Remove(trackerDto.TrackerDtoId);
        }, cancellationToken);
    }

    public async Task<IEnumerable<TrackerDto>?> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            var trackers = stuff.Trackers.Values.Where(t => t.RoomDtoId == roomId).ToImmutableArray();
            return trackers;
        }, cancellationToken);
    }

    public async Task<TrackerDto?> GetTrackerByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.Trackers.TryGetValue(id, out var trackerDto) ? trackerDto : null;
        }, cancellationToken);
    }

    public async Task<TrackerDto?> GetTrackerByMacAddresses(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.Trackers.Values.SingleOrDefault(t =>
                t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress);
        }, cancellationToken);
    }

    public async Task<IEnumerable<UnregisteredTrackerDto>> GetUnregisteredTrackers(CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.UnregisteredTrackers.ToImmutableArray();
        }, cancellationToken);
    }

    public async Task AddUnregisteredTracker(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        await LockedStuffMutex.Lock(stuff =>
        {
            stuff.UnregisteredTrackers.Add(
                new UnregisteredTrackerDto()
                {
                    WifiMacAddress = wifiMacAddress,
                    BluetoothMacAddress = bluetoothMacAddress
                });
        }, cancellationToken);
    }

    public async Task<IEnumerable<TrackerDto>> GetRegisteredTrackers(CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.Trackers.Values.ToImmutableArray();
        }, cancellationToken);
    }
}