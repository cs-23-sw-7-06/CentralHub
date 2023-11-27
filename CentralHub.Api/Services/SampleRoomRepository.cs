using System.Collections.Immutable;
using CentralHub.Api.Dtos;

namespace CentralHub.Api.Services;

public sealed class SampleRoomRepository : IRoomRepository
{
    private static readonly Dictionary<int, RoomDto> Rooms = new Dictionary<int, RoomDto>();
    private static int _nextId;

    private static readonly object LockObject = new object();

    static SampleRoomRepository()
    {
        var sampleRoomRepository = new SampleRoomRepository();
        var room1 = new RoomDto()
        {
            Name = "Sample Room 1",
            Description = "Sample Room",
            RoomDtoId = -1,
            Trackers = new List<TrackerDto>()
        };

        var room2 = new RoomDto()
        {
            Name = "Sample Room 2",
            Description = "Sample Room",
            RoomDtoId = -1,
            Trackers = new List<TrackerDto>()
        };

        sampleRoomRepository.AddRoomAsync(room1, default).GetAwaiter().GetResult();
        sampleRoomRepository.AddRoomAsync(room2, default).GetAwaiter().GetResult();
    }

    public Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            roomDto.RoomDtoId = _nextId++;
            Rooms.Add(roomDto.RoomDtoId, roomDto);
        }

        return Task.FromResult(roomDto.RoomDtoId);
    }

    public Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        // Nothing needs to be done.

        return Task.CompletedTask;
    }

    public Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            Rooms.Remove(roomDto.RoomDtoId);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            return Task.FromResult((IEnumerable<RoomDto>)Rooms.Values.ToImmutableArray());
        }
    }

    public Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        lock (LockObject)
        {
            if (Rooms.TryGetValue(id, out var roomDto))
            {
                return Task.FromResult<RoomDto?>(roomDto);
            }
        }

        return Task.FromResult<RoomDto?>(null);
    }
}