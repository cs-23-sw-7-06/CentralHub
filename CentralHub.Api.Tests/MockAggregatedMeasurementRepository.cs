using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Responses.Measurement;
using CentralHub.Api.Services;
using CentralHub.Api.Threading;

namespace CentralHub.Api.Tests;

internal sealed class MockAggregatedMeasurementRepository : IMeasurementRepository
{
    private static readonly CancellableMutex<Dictionary<int, List<MeasurementGroup>>> RecentMeasurementGroupsMutex =
        new CancellableMutex<Dictionary<int, List<MeasurementGroup>>>(new Dictionary<int, List<MeasurementGroup>>());


    private static readonly CancellableMutex<AggregatedMeasurementsStuff> AggregatedMeasurementsMutex =
        new CancellableMutex<AggregatedMeasurementsStuff>(new AggregatedMeasurementsStuff());

    private sealed class AggregatedMeasurementsStuff
    {
        public List<AggregatedMeasurementDto> AggregatedMeasurements { get; } = new List<AggregatedMeasurementDto>();

        public int NextId { get; set; } = 0;
    }

    public async Task<int> AddAggregatedMeasurementAsync(AggregatedMeasurementDto aggregatedDto, CancellationToken cancellationToken)
    {
        return await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            stuff.AggregatedMeasurements.Add(aggregatedDto);
            aggregatedDto.AggregatedMeasurementDtoId = stuff.NextId;
            stuff.NextId++;

            return aggregatedDto.AggregatedMeasurementDtoId;
        }, cancellationToken);
    }

    public async Task RemoveAggregatedMeasurementAsync(AggregatedMeasurementDto aggregatedDto, CancellationToken cancellationToken)
    {
        await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            stuff.AggregatedMeasurements.Remove(aggregatedDto);
        }, cancellationToken);
    }

    public async Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, CancellationToken cancellationToken)
    {
        return await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            return stuff.AggregatedMeasurements
                .Where(m => m.RoomDtoId == roomId)
                .ToImmutableArray();
        }, cancellationToken);
    }

    public async Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        return await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            return stuff.AggregatedMeasurements
                .Where(m => m.RoomDtoId == roomId && m.StartTime >= startTime && m.EndTime <= endTime)
                .ToImmutableArray();
        }, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetRoomMeasurementGroupsAsync(CancellationToken cancellationToken)
    {
        return await RecentMeasurementGroupsMutex.Lock(recentMeasurementGroups =>
        {
            var recentTrackerMeasurementGroups = recentMeasurementGroups
                .ToDictionary(k => k.Key, v => (IReadOnlyList<MeasurementGroup>)v.Value.ToImmutableArray());
            recentMeasurementGroups.Clear();
            return recentTrackerMeasurementGroups;
        }, cancellationToken);
    }

    public async Task AddMeasurementsAsync(int id, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken)
    {
        await RecentMeasurementGroupsMutex.Lock(recentMeasurementGroups =>
        {
            if (recentMeasurementGroups.TryGetValue(id, out var value))
            {
                value.Add(new MeasurementGroup(measurements.ToImmutableArray()));
            }
            else
            {
                recentMeasurementGroups.Add(id,
                    new List<MeasurementGroup>() { new MeasurementGroup(measurements.ToImmutableArray()) });
            }

        }, cancellationToken);
    }

    public async Task<DateTime?> GetFirstAggregatedMeasurementsDateTimeAsync(int roomId, CancellationToken cancellationToken)
    {
        return await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            var aggregatedMeasurements = stuff.AggregatedMeasurements.Where(m => m.RoomDtoId == roomId)
                .ToImmutableArray();
            if (!aggregatedMeasurements.Any())
            {
                return (DateTime?)null;
            }

            return aggregatedMeasurements.Min(m => m.StartTime);
        }, cancellationToken);
    }

    public async Task AddAggregatedMeasurementAsync(List<AggregatedMeasurementDto> measurementDtos, CancellationToken cancellationToken)
    {
        await AggregatedMeasurementsMutex.Lock(stuff =>
        {
            stuff.AggregatedMeasurements.AddRange(measurementDtos);
            measurementDtos.ForEach(am =>
            {
                am.AggregatedMeasurementDtoId = stuff.NextId;
                stuff.NextId++;
            });
        }, cancellationToken);
    }
}