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

        var rooms = new List<RoomDto>();

        for (int i = 0; i < 5; i++)
        {
            rooms.Add(sampleRoomRepository.GetRoomByIdAsync(i, default).GetAwaiter().GetResult()!);
        }

        var trackers = new List<TrackerDto>();

        for (int i = 0; i < 5; i++)
        {
            string wifiName;
            string bluetoothName;
            if (i < 10)
            {
                wifiName = $"00:00:00:00:FE:0{i}";
                bluetoothName = $"00:00:00:00:0{i}:BL";
            }
            else
            {
                wifiName = $"00:00:00:00:FE:{i}";
                bluetoothName = $"00:00:00:00:{i}:BL";
            }

            trackers.Add(new TrackerDto()
            {
                WifiMacAddress = wifiName,
                BluetoothMacAddress = bluetoothName,
                Name = $"Sample Tracker {i}",
                Description = "Sample Tracker",
                RoomDtoId = rooms[i % 1].RoomDtoId,
                RoomDto = rooms[i % 1],
                TrackerDtoId = -1,
            });
        }


        foreach (var tracker in trackers)
        {
            sampleTrackerRepository.AddTrackerAsync(tracker, default).GetAwaiter().GetResult();
        }

        for (int i = 0; i < 5; i++)
        {
            string wifiName;
            string bluetoothName;
            if (i < 10)
            {
                wifiName = $"0{i}:FE:00:00:00:00";
                bluetoothName = $"BL:0{i}:00:00:00:00";
            }
            else
            {
                wifiName = $"{i}:FE:00:00:00:00";
                bluetoothName = $"BL:{i}:00:00:00:00";
            }

            sampleTrackerRepository.AddUnregisteredTracker(wifiName, bluetoothName, default).GetAwaiter().GetResult();
        }
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
            stuff.UnregisteredTrackers.Add(new UnregisteredTrackerDto()
            {
                WifiMacAddress = trackerDto.WifiMacAddress,
                BluetoothMacAddress = trackerDto.BluetoothMacAddress
            });
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