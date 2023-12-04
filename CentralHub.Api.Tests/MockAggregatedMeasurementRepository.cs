using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

internal sealed class MockAggregatedMeasurementRepository : IMeasurementRepository
{
    private static readonly Dictionary<int, List<MeasurementGroup>> _recentMeasurementGroups = new Dictionary<int, List<MeasurementGroup>>();

    private static readonly List<AggregatedMeasurementDto> _AggregatedMeasurements = new List<AggregatedMeasurementDto>();
    private int _nextId = 0;

    public Task<int> AddAggregatedMeasurementAsync(AggregatedMeasurementDto aggregatedDto, CancellationToken cancellationToken)
    {
        lock (_AggregatedMeasurements)
        {
            _AggregatedMeasurements.Add(aggregatedDto);
            aggregatedDto.AggregatedMeasurementDtoId = _nextId;
            _nextId++;
        }
        return new ValueTask<int>(aggregatedDto.RoomDtoId).AsTask();
    }

    public Task RemoveAggregatedMeasurementAsync(AggregatedMeasurementDto aggregatedDto, CancellationToken cancellationToken)
    {
        lock (_AggregatedMeasurements)
        {
            _AggregatedMeasurements.Remove(aggregatedDto);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, CancellationToken cancellationToken)
    {
        lock (_AggregatedMeasurements)
        {
            return new ValueTask<IEnumerable<AggregatedMeasurementDto>>(
                _AggregatedMeasurements
                .Where(m => m.RoomDtoId == roomId)
                .ToImmutableArray()).AsTask();
        }
    }

    public Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken cancellationToken)
    {
        lock (_AggregatedMeasurements)
        {
            return new ValueTask<IEnumerable<AggregatedMeasurementDto>>(
                _AggregatedMeasurements
                .Where(m => m.RoomDtoId == roomId && m.StartTime >= timeStart && m.EndTime <= timeEnd)
                .ToImmutableArray()).AsTask();
        }
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetRoomMeasurementGroupsAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            lock (_recentMeasurementGroups)
            {
                var recentTrackerMeasurementGroups = _recentMeasurementGroups
                    .ToDictionary(k => k.Key, v => (IReadOnlyList<MeasurementGroup>)v.Value.ToImmutableArray());
                _recentMeasurementGroups.Clear();
                return recentTrackerMeasurementGroups;
            }
        }, cancellationToken);
    }

    public Task AddMeasurementsAsync(int id, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            lock (_recentMeasurementGroups)
            {
                if (_recentMeasurementGroups.TryGetValue(id, out var value))
                {
                    value.Add(new MeasurementGroup(measurements.ToList()));
                }
                else
                {
                    _recentMeasurementGroups.Add(id, new List<MeasurementGroup>() { new MeasurementGroup(measurements.ToList()) });
                }
            }
        }, cancellationToken);
    }
}