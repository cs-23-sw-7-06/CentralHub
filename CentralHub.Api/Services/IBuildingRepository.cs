using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface IBuildingRepository
{
    Task AddBuildingAsync(Building building, CancellationToken cancellationToken);

    Task UpdateBuildingAsync(Building building, CancellationToken cancellationToken);

    Task RemoveBuildingAsync(Building building, CancellationToken cancellationToken);

    Task AddRoomAsync(Room room, CancellationToken cancellationToken);

    Task UpdateRoomAsync(Room room, CancellationToken cancellationToken);

    Task RemoveRoomAsync(Room room, CancellationToken cancellationToken);

    Task<IEnumerable<Building>> GetBuildingsAsync(CancellationToken cancellationToken);

    Task<Building?> GetBuildingByIdAsync(int id, CancellationToken cancellationToken);
}
