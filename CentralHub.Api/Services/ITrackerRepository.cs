using CentralHub.Api.Dtos;

namespace CentralHub.Api.Services;

public interface ITrackerRepository
{
    Task<int> AddTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken);

    Task UpdateTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken);

    Task RemoveTrackerAsync(TrackerDto trackerDto, CancellationToken cancellationToken);

    Task<IEnumerable<TrackerDto>?> GetTrackersInRoomAsync(int roomId, CancellationToken cancellationToken);

    Task<TrackerDto?> GetTrackerByIdAsync(int id, CancellationToken cancellationToken);

    Task<TrackerDto?> GetTrackerByMacAddressesAsync(string wifiMacAddress, string bluetoothMacAddress,
        CancellationToken cancellationToken);

    Task<IEnumerable<TrackerDto>> GetRegisteredTrackersAsync(CancellationToken cancellationToken);

    Task<IEnumerable<UnregisteredTrackerDto>> GetUnregisteredTrackersAsync(CancellationToken cancellationToken);

    Task AddUnregisteredTrackerAsync(string wifiMacAddress, string bluetoothMacAddress, CancellationToken cancellationToken);
}