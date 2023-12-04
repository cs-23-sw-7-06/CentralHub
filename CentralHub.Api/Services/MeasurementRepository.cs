using System.Collections.Immutable;
using CentralHub.Api.DbContexts;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Threading;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

internal sealed class MeasurementRepository : IMeasurementRepository
{
    private static readonly CancellableMutex<Dictionary<int, List<MeasurementGroup>>> RecentRoomMeasurementGroupsMutex =
        new CancellableMutex<Dictionary<int, List<MeasurementGroup>>>(new Dictionary<int, List<MeasurementGroup>>());

    private readonly ApplicationDbContext _applicationDbContext;

    public MeasurementRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _applicationDbContext.Database.OpenConnection();
        _applicationDbContext.Database.EnsureCreated();
    }

    public async Task<int> AddAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken)
    {
        _applicationDbContext.AggregatedMeasurements.Add(measurementDto);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return measurementDto.AggregatedMeasurementDtoId;
        }
        catch (OperationCanceledException)
        {
            // Remove the roomDto from the collection as the operation was cancelled.
            _applicationDbContext.AggregatedMeasurements.Remove(measurementDto);
            throw;
        }
    }

    public async Task RemoveAggregatedMeasurementAsync(AggregatedMeasurementDto measurementDto, CancellationToken cancellationToken)
    {
        _applicationDbContext.AggregatedMeasurements.Remove(measurementDto);
        try
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // add the roomDto from the collection as the operation was cancelled.
            _applicationDbContext.AggregatedMeasurements.Add(measurementDto);
            throw;
        }
    }

    public async Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.AggregatedMeasurements.Where(am => roomId == am.RoomDtoId).ToArrayAsync(cancellationToken);
    }

    public async Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.AggregatedMeasurements
            .Where(am => roomId == am.RoomDtoId)
            .Where(am => am.StartTime >= startTime && am.EndTime <= endTime)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddMeasurementsAsync(int roomId, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken)
    {
        await RecentRoomMeasurementGroupsMutex.Lock(recentRoomMeasurementGroups =>
        {
            if (recentRoomMeasurementGroups.TryGetValue(roomId, out var value))
            {
                value.Add(new MeasurementGroup(measurements.ToImmutableArray()));
            }
            else
            {
                recentRoomMeasurementGroups.Add(roomId, new List<MeasurementGroup>() { new MeasurementGroup(measurements.ToImmutableArray()) });
            }
        }, cancellationToken);
    }

    public async Task<DateTime?> GetFirstAggregatedMeasurementsDateTimeAsync(int roomId, CancellationToken cancellationToken)
    {
        var aggregatedMeasurements = _applicationDbContext.AggregatedMeasurements.Where(m => m.RoomDtoId == roomId);
        if (!aggregatedMeasurements.Any())
        {
            return null;
        }

        return await aggregatedMeasurements.MinAsync(m => m.StartTime, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetRoomMeasurementGroupsAsync(CancellationToken cancellationToken)
    {
        return await RecentRoomMeasurementGroupsMutex.Lock(recentRoomMeasurementGroups =>
        {
            var copy = recentRoomMeasurementGroups.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MeasurementGroup>)kv.Value.ToImmutableArray());
            recentRoomMeasurementGroups.Clear();

            return copy;
        }, cancellationToken);
    }
}