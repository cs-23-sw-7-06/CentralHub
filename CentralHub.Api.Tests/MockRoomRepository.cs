using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;
internal sealed class MockRoomRepository : IRoomRepository
{
    private readonly List<RoomDto> _rooms = new List<RoomDto>();
    private int _nextId = 0;

    public Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        _rooms.Add(roomDto);
        roomDto.RoomDtoId = _nextId;
        _nextId++;

        return new ValueTask<int>(roomDto.RoomDtoId).AsTask();
    }

    public Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        _rooms.Remove(roomDto);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<IEnumerable<RoomDto>>(_rooms).AsTask();
    }

    public Task<RoomDto> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_rooms.SingleOrDefault(r => r.RoomDtoId == id));
    }
}