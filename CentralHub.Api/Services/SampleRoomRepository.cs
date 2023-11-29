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
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.Rooms.Values.ToImmutableArray();
        }, cancellationToken);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await LockedStuffMutex.Lock(stuff =>
        {
            return stuff.Rooms.TryGetValue(id, out var roomDto) ? roomDto : null;
        }, cancellationToken);
    }
}