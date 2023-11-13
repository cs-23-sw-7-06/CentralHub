using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface IRoomRepository
{
    Task AddRoomAsync(Room room, CancellationToken cancellationToken);

    Task UpdateRoomAsync(Room room, CancellationToken cancellationToken);

    Task RemoveRoomAsync(Room room, CancellationToken cancellationToken);

    Task AddTrackerAsync(Tracker tracker, CancellationToken cancellationToken);

    Task UpdateTrackerAsync(Tracker tracker, CancellationToken cancellationToken);

    Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken);

    Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken cancellationToken);

    Task<Room?> GetRoomByIdAsync(int id, CancellationToken cancellationToken);
    Task AddMeasurementsAsync(MeasurementCollection measurements, CancellationToken token);
    Task<ICollection<Measurement>> GetMeasurementsAsync(int id, CancellationToken token);
}