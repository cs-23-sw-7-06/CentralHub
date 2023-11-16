using System.ComponentModel.DataAnnotations;
using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CentralHub.Api.Services;

internal sealed class RoomRepository : IRoomRepository
{
    private static volatile bool _hasAddedTrackers = false;
    private readonly ApplicationDbContext _applicationDbContext;

    public RoomRepository(ApplicationDbContext applicationDbContext)
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
        var room = new Room("Test Room 1", "Test Room");
        AddRoomAsync(room, default).GetAwaiter().GetResult();
        AddTrackerAsync(new Tracker("Test Tracker 1", "Test Tracker", "AA:BB:CC:DD:EE:FF", room), default).GetAwaiter().GetResult();
        AddTrackerAsync(new Tracker("Test Tracker 2", "Test Tracker", "FF:EE:DD:CC:BB:AA", room), default).GetAwaiter().GetResult();
        AddTrackerAsync(new Tracker("Test Tracker 3", "Test Tracker", "00:11:22:33:44:55", room), default).GetAwaiter().GetResult();
        _hasAddedTrackers = true;
#endif
    }

    public async Task AddRoomAsync(Room room, CancellationToken cancellationToken)
    {
        _applicationDbContext.Rooms.Add(room);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Remove the room from the collection as the operation was cancelled.
            _applicationDbContext.Rooms.Remove(room);
            throw;
        }
    }

    public async Task UpdateRoomAsync(Room room, CancellationToken cancellationToken)
    {
        // TODO: Undo update if cancellation was requested
        _applicationDbContext.Rooms.Update(room);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRoomAsync(Room room, CancellationToken cancellationToken)
    {
        _applicationDbContext.Rooms.Remove(room);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Undo delete if cancelled.
            _applicationDbContext.Rooms.Add(room);
            throw;
        }
    }

    public async Task AddTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
    {
        var room = tracker.Room;
        room.Trackers.Add(tracker);
        _applicationDbContext.Rooms.Update(room);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Set the room to unchanged
            _applicationDbContext.Rooms.Attach(room).State = EntityState.Unchanged;
            room.Trackers.Remove(tracker);
            throw;
        }
    }

    public async Task UpdateTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
    {
        await UpdateRoomAsync(tracker.Room, cancellationToken);
    }

    public async Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
    {
        var room = tracker.Room;
        room.Trackers.Add(tracker);
        _applicationDbContext.Rooms.Update(room);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Set the room to unchanged
            _applicationDbContext.Rooms.Attach(room).State = EntityState.Unchanged;
            room.Trackers.Remove(tracker);
            throw;
        }
    }

    public async Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Rooms.ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Returns null if the room is not found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Room?> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Rooms.SingleOrDefaultAsync(r => r.RoomId == id, cancellationToken);
    }


    public List<KeyValuePair<int, Measurement>> Measurements { get; set; }

    async Task IRoomRepository.AddMeasurementsAsync(MeasurementCollection measurements, CancellationToken token)
    {
        foreach (var measurement in measurements.Measurements)
        {
            Measurements.Add(new KeyValuePair<int, Measurement>(measurements.TrackerId, measurement));
        }


    }

    async Task<ICollection<Measurement>> IRoomRepository.GetMeasurementsAsync(int id, CancellationToken token)
    {

        if (Measurements.Where(measurement => measurement.Key == id).Count() > 0)
        {
            var measurementsForTracker = Measurements.Where(measurement => measurement.Key == id).Select(pair => pair.Value).ToList();
            return await Task.FromResult((ICollection<Measurement>)measurementsForTracker);
        }
        else
        {
            var measurementsForTracker = new List<Measurement>();
            return measurementsForTracker;
        }
    }
}