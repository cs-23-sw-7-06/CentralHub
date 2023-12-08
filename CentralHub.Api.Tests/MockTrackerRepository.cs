using CentralHub.Api.Dtos;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

internal sealed class MockTrackerRepository : ITrackerRepository
{
    private readonly List<UnregisteredTrackerDto> _unregisteredTrackers = new List<UnregisteredTrackerDto>();

    private int _nextId = 0;

    public RoomDto RoomDto { get; }

    public MockTrackerRepository(RoomDto roomDto)
    {
        RoomDto = roomDto;
    }

    public Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        if (trackerDto.RoomDtoId != RoomDto.RoomDtoId)
        {
            throw new KeyNotFoundException("RoomDto not found");
        }

        trackerDto.TrackerDtoId = _nextId;
        trackerDto.RoomDto = RoomDto;

        _nextId++;

        RoomDto.Trackers.Add(trackerDto);

        return new ValueTask<int>(trackerDto.TrackerDtoId).AsTask();
    }

    public Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken)
    {
        if (trackerDto.RoomDtoId != RoomDto.RoomDtoId)
        {
            throw new KeyNotFoundException("RoomDto not found");
        }

        RoomDto.Trackers.Remove(trackerDto);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TrackerDto>> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken)
    {
        if (roomId != RoomDto.RoomDtoId)
        {
            return Task.FromResult<IEnumerable<TrackerDto>>(null);
        }

        return Task.FromResult(RoomDto.Trackers.AsEnumerable());
    }

    public Task<TrackerDto> GetTrackerByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(RoomDto.Trackers.SingleOrDefault(t => t.TrackerDtoId == id));
    }

    public Task<TrackerDto> GetTrackerByMacAddressesAsync(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        return Task.FromResult(RoomDto.Trackers.SingleOrDefault(t =>
            t.WifiMacAddress == wifiMacAddress && t.BluetoothMacAddress == bluetoothMacAddress));
    }

    public Task<IEnumerable<UnregisteredTrackerDto>> GetUnregisteredTrackersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<UnregisteredTrackerDto>>(_unregisteredTrackers.ToArray());
    }

    public Task AddUnregisteredTrackerAsync(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        _unregisteredTrackers.Add(
            new UnregisteredTrackerDto
            {
                WifiMacAddress = wifiMacAddress,
                BluetoothMacAddress = bluetoothMacAddress,
            });
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TrackerDto>> GetRegisteredTrackersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(RoomDto.Trackers.ToArray().AsEnumerable());
    }
}
