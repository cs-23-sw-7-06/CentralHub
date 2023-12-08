using System.Collections.Immutable;
using CentralHub.Api.DbContexts;
using CentralHub.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

internal sealed class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    private readonly ITrackerRepository _trackerRepository;

    public RoomRepository(ApplicationDbContext applicationDbContext, ITrackerRepository trackerRepository)
    {
        _applicationDbContext = applicationDbContext;
        _applicationDbContext.Database.OpenConnection();
        _applicationDbContext.Database.EnsureCreated();

        _trackerRepository = trackerRepository;
    }

    public async Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        _applicationDbContext.Rooms.Add(roomDto);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return roomDto.RoomDtoId;
        }
        catch (OperationCanceledException)
        {
            // Remove the roomDto from the collection as the operation was cancelled.
            _applicationDbContext.Rooms.Remove(roomDto);
            throw;
        }
    }

    public async Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        // TODO: Undo update if cancellation was requested
        _applicationDbContext.Rooms.Update(roomDto);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        var trackers = (await _applicationDbContext.Rooms
            .Include(r => r.Trackers)
            .SingleAsync(r => r == roomDto, cancellationToken))
            .Trackers.ToImmutableArray();

        _applicationDbContext.Rooms.Remove(roomDto);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Undo delete if cancelled.
            _applicationDbContext.Rooms.Add(roomDto);
            throw;
        }

        Task.WaitAll(trackers.Select(tracker => _trackerRepository.AddUnregisteredTrackerAsync(tracker.WifiMacAddress, tracker.BluetoothMacAddress, cancellationToken)).ToArray(), cancellationToken);
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        // NOTE: Does not include trackers
        return await _applicationDbContext.Rooms.ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Returns null if the roomDto is not found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Rooms.SingleOrDefaultAsync(r => r.RoomDtoId == id, cancellationToken);
    }
}