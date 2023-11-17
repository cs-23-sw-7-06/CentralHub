using CentralHub.Api.Dtos;

namespace CentralHub.Api.Services;

public interface IRoomRepository
{
    Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken);

    Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken);

    Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken);

    Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken);

    Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken cancellationToken);
}
