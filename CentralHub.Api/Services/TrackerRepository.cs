using CentralHub.Api.DbContexts;
using CentralHub.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

public sealed class TrackerRepository : ITrackerRepository
{
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
            Description = "Test RoomDto"
        };

        _applicationDbContext.Rooms.Add(room);
        _applicationDbContext.SaveChangesAsync().GetAwaiter().GetResult();
        AddTrackerAsync(new TrackerDto()
        {
            Name = "Test TrackerDto 1",
            Description = "Test TrackerDto",
            MacAddress = "AA:BB:CC:DD:EE:FF",
            RoomDtoId = room.RoomDtoId,
            RoomDto = room
        }, default).GetAwaiter().GetResult();
        AddTrackerAsync(new TrackerDto()
        {
            Name = "Test TrackerDto 2",
            Description = "Test TrackerDto",
            MacAddress = "AA:BB:CC:DD:EE:FF",
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

    public async Task<TrackerDto?> GetTrackerAsync(int id, CancellationToken cancellationToken)
    {
        var tracker = await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SelectMany(r => r.Trackers)
            .SingleOrDefaultAsync(t => t.TrackerDtoId == id, cancellationToken);

        return tracker;
    }
}