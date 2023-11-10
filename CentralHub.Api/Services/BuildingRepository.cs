using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

internal sealed class BuildingRepository : IBuildingRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public BuildingRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _applicationDbContext.Database.OpenConnection();
        _applicationDbContext.Database.EnsureCreated();
    }

    public async Task AddBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        _applicationDbContext.Buildings.Add(building);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Remove the building from the collection as the operation was cancelled.
            _applicationDbContext.Buildings.Remove(building);
            throw;
        }
    }

    public async Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        // TODO: Undo update if cancellation was requested
        _applicationDbContext.Buildings.Update(building);
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveBuildingAsync(Building building, CancellationToken cancellationToken)
    {
        _applicationDbContext.Buildings.Remove(building);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Undo delete if cancelled.
            _applicationDbContext.Buildings.Add(building);
            throw;
        }
    }

    public async Task AddRoomAsync(Room room, CancellationToken cancellationToken)
    {
        var building = room.Building;
        building.Rooms.Add(room);
        _applicationDbContext.Buildings.Update(building);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Set the building to unchanged
            _applicationDbContext.Buildings.Attach(building).State = EntityState.Unchanged;
            building.Rooms.Remove(room);
            throw;
        }
    }

    public async Task UpdateRoomAsync(Room room, CancellationToken cancellationToken)
    {
        await UpdateBuildingAsync(room.Building, cancellationToken);
    }

    public async Task RemoveRoomAsync(Room room, CancellationToken cancellationToken)
    {
        var building = room.Building;
        building.Rooms.Add(room);
        _applicationDbContext.Buildings.Update(building);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Set the building to unchanged
            _applicationDbContext.Buildings.Attach(building).State = EntityState.Unchanged;
            building.Rooms.Remove(room);
            throw;
        }
    }

    public async Task<IEnumerable<Building>> GetBuildingsAsync(CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Buildings.ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Returns null if the building is not found.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Building?> GetBuildingByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Buildings.SingleOrDefaultAsync(r => r.BuildingId == id, cancellationToken);
    }
}
