using CentralHub.Api.Dtos;
using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface IAggregatedMeasurementRepository
{
    Task<int> AddMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task RemoveMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetMeasurementsAsync(int roomId, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetMeasurementsAsync(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetTrackerMeasurementGroupsAsync(CancellationToken cancellationToken);

    Task AddMeasurementsAsync(int trackerId, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken);
}