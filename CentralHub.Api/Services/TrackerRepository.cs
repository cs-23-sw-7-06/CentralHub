using System.Collections.Immutable;
using CentralHub.Api.DbContexts;
using CentralHub.Api.Dtos;
using CentralHub.Api.Threading;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

public sealed class TrackerRepository : ITrackerRepository
{
    private static readonly CancellableMutex<List<UnregisteredTrackerDto>> UnregisteredTrackersMutex = new CancellableMutex<List<UnregisteredTrackerDto>>(new List<UnregisteredTrackerDto>());

    private readonly ApplicationDbContext _applicationDbContext;
    private static bool _hasAddedTrackers;

    public TrackerRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _applicationDbContext.Database.OpenConnection();
        _applicationDbContext.Database.EnsureCreated();

#if DEBUG
        if (_hasAddedTrackers)
        {
            return;
        }
        // Insert dummy trackers
        var room = new RoomDto()
        {
            Name = "Test RoomDto 1",
            Description = "Test RoomDto",
            Capacity = 10,
            NeighbouringRooms = [],
        };

        _applicationDbContext.Rooms.Add(room);
        _applicationDbContext.SaveChangesAsync().GetAwaiter().GetResult();
        AddTrackerAsync(new TrackerDto()
        {
            Name = "Test TrackerDto 1",
            Description = "Test TrackerDto",
            WifiMacAddress = "AA:BB:CC:DD:EE:FF",
            BluetoothMacAddress = "00:11:22:33:44:55",
            RoomDtoId = room.RoomDtoId,
            RoomDto = room
        }, default).GetAwaiter().GetResult();
        AddTrackerAsync(new TrackerDto()
        {
            Name = "Test TrackerDto 2",
            Description = "Test TrackerDto",
            WifiMacAddress = "FF:EE:DD:CC:BB:AA",
            BluetoothMacAddress = "55:44:33:22:11:00",
            RoomDtoId = room.RoomDtoId,
            RoomDto = room
        }, default).GetAwaiter().GetResult();
        _hasAddedTrackers = true;
#endif
    }

    public async Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        var room = trackerDto.RoomDto;
        if (room == null)
        {
            throw new InvalidOperationException("trackerDto.RoomDto was null");
        }

        room.Trackers.Add(trackerDto);
        _applicationDbContext.Rooms.Update(room);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            await UnregisteredTrackersMutex.Lock(unregisteredTrackers =>
            {
                var possibleUnregisteredTracker = unregisteredTrackers.SingleOrDefault(t =>
                    t.WifiMacAddress == trackerDto.WifiMacAddress &&
                    t.BluetoothMacAddress == trackerDto.BluetoothMacAddress);

                if (possibleUnregisteredTracker != null)
                {
                    unregisteredTrackers.Remove(possibleUnregisteredTracker);
                }
            }, cancellationToken);
            return trackerDto.TrackerDtoId;
        }
        catch (OperationCanceledException)
        {
            // Set the roomDto to unchanged
            _applicationDbContext.Rooms.Attach(room).State = EntityState.Unchanged;
            room.Trackers.Remove(trackerDto);
            throw;
        }
    }

    public async Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        if (trackerDto.RoomDto == null)
        {
            throw new InvalidOperationException("trackerDto.RoomDto was null");
        }

        _applicationDbContext.Rooms.Update(trackerDto.RoomDto);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        var room = trackerDto.RoomDto;
        if (room == null)
        {
            throw new InvalidOperationException("trackerDto.RoomDto was null");
        }

        room.Trackers.Remove(trackerDto);
        var unregisteredTracker = new UnregisteredTrackerDto()
        {
            BluetoothMacAddress = trackerDto.BluetoothMacAddress,
            WifiMacAddress = trackerDto.WifiMacAddress,
        };

        await UnregisteredTrackersMutex.Lock(unregisteredTrackers =>
        {
            unregisteredTrackers.Add(unregisteredTracker);
        }, cancellationToken);
        _applicationDbContext.Rooms.Update(room);

        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Set the roomDto to unchanged
            _applicationDbContext.Rooms.Attach(room).State = EntityState.Unchanged;
            room.Trackers.Add(trackerDto);
            await UnregisteredTrackersMutex.Lock(unregisteredTrackers =>
            {
                unregisteredTrackers.Remove(unregisteredTracker);
            }, cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<TrackerDto>?> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken)
    {
        var room = await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SingleOrDefaultAsync(r => r.RoomDtoId == roomId, cancellationToken);

        if (room == null)
        {
            return null;
        }

        return room.Trackers.ToArray();
    }

    public async Task<TrackerDto?> GetTrackerByIdAsync(int id, CancellationToken cancellationToken)
    {
        var tracker = await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SelectMany(r => r.Trackers)
            .Include(t => t.RoomDto)
            .SingleOrDefaultAsync(t => t.TrackerDtoId == id, cancellationToken);

        return tracker;
    }

    public async Task<TrackerDto?> GetTrackerByMacAddressesAsync(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        var tracker = await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SelectMany(r => r.Trackers)
            .SingleOrDefaultAsync(t =>
                t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress, cancellationToken);

        return tracker;
    }

    public async Task<IEnumerable<TrackerDto>> GetRegisteredTrackersAsync(CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SelectMany(r => r.Trackers)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IEnumerable<UnregisteredTrackerDto>> GetUnregisteredTrackersAsync(CancellationToken cancellationToken)
    {
        return await UnregisteredTrackersMutex.Lock(
            unregisteredTrackers => unregisteredTrackers.ToImmutableArray(),
            cancellationToken);
    }

    public async Task AddUnregisteredTrackerAsync(string wifiMacAddress, string bluetoothMacAddress,
        CancellationToken cancellationToken)
    {
        await UnregisteredTrackersMutex.Lock(unregisteredTrackers =>
        {
            if (unregisteredTrackers.SingleOrDefault(t =>
                    t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress) == null)
            {
                unregisteredTrackers.Add(new UnregisteredTrackerDto()
                {
                    WifiMacAddress = wifiMacAddress,
                    BluetoothMacAddress = bluetoothMacAddress,
                });
            }
        }, cancellationToken);
    }
}