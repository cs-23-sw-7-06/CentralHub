using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

internal sealed class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public RoomRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _applicationDbContext.Database.OpenConnection();
        _applicationDbContext.Database.EnsureCreated();
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
}