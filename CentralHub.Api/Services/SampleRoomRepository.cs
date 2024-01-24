using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Threading;

namespace CentralHub.Api.Services;

public sealed class SampleRoomRepository : IRoomRepository
{
    private sealed class LockedStuff
    {
        public Dictionary<int, RoomDto> Rooms { get; } = new Dictionary<int, RoomDto>();
        public int NextId { get; set; }
    }

    private static readonly CancellableMutex<LockedStuff> LockedStuffMutex = new CancellableMutex<LockedStuff>(new LockedStuff());

    static SampleRoomRepository()
    {
        var sampleRoomRepository = new SampleRoomRepository();
        var rooms = new List<RoomDto>();

        for (int i = 0; i < 5; i++)
        {
            rooms.Add(new RoomDto()
            {
                Name = $"Sample Room {i}",
                Description = "Sample Room",
                Capacity = 10,
                NeighbouringRooms = [],
                RoomDtoId = -1,
                Trackers = new List<TrackerDto>()
            });
        }

        foreach (var room in rooms)
        {
            sampleRoomRepository.AddRoomAsync(room, default).GetAwaiter().GetResult();
        }
    }

    public async Task<int> AddRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        await LockedStuffMutex.Lock(stuff =>
        {
            roomDto.RoomDtoId = stuff.NextId++;
            stuff.Rooms.Add(roomDto.RoomDtoId, roomDto);
        }, cancellationToken);

        return roomDto.RoomDtoId;
    }

    public Task UpdateRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        // Nothing needs to be done.

        return Task.CompletedTask;
    }

    public async Task RemoveRoomAsync(RoomDto roomDto, CancellationToken cancellationToken)
    {
        await LockedStuffMutex.Lock(stuff =>
        {
            stuff.Rooms.Remove(roomDto.RoomDtoId);
        }, cancellationToken);
    }

    public async Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(
            stuff => stuff.Rooms.Values.ToImmutableArray(),
            cancellationToken);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(
            stuff => stuff.Rooms.TryGetValue(id, out var roomDto) ? roomDto : null,
            cancellationToken);
    }
}