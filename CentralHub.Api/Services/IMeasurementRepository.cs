using CentralHub.Api.Dtos;
using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface IMeasurementRepository
{
    Task<int> AddAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task RemoveAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetTrackerMeasurementGroupsAsync(CancellationToken cancellationToken);

    Task AddMeasurementsAsync(int trackerId, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken);
}