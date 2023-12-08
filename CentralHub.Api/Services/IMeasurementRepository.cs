using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Responses.Measurement;

namespace CentralHub.Api.Services;

public interface IMeasurementRepository
{
    Task<int> AddAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task RemoveAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, CancellationToken cancellationToken);

    Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetRoomMeasurementGroupsAsync(CancellationToken cancellationToken);

    Task AddMeasurementsAsync(int roomId, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken);

    Task<DateTime?> GetFirstAggregatedMeasurementsDateTimeAsync(int roomId, CancellationToken cancellationToken);
}