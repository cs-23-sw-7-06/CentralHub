using System.Collections.Immutable;
using CentralHub.Api.DbContexts;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

internal sealed class MeasurementRepository : IMeasurementRepository
{
    private readonly static Dictionary<int, List<MeasurementGroup>> _recentTrackerMeasurementGroups = new Dictionary<int, List<MeasurementGroup>>();

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

    public async Task<IEnumerable<AggregatedMeasurementDto>> GetAggregatedMeasurementsAsync(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.AggregatedMeasurements
            .Where(am => roomId == am.RoomDtoId)
            .Where(am => am.StartTime >= timeStart && am.EndTime <= timeEnd)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddMeasurementsAsync(int trackerId, IReadOnlyCollection<Measurement> measurements, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            lock (_recentTrackerMeasurementGroups)
            {
                if (_recentTrackerMeasurementGroups.TryGetValue(trackerId, out var value))
                {
                    value.Add(new MeasurementGroup(measurements.ToImmutableArray()));
                }
                else
                {
                    _recentTrackerMeasurementGroups.Add(trackerId, new List<MeasurementGroup>() { new MeasurementGroup(measurements.ToImmutableArray()) });
                }
            }
        }, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<MeasurementGroup>>> GetTrackerMeasurementGroupsAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            lock (_recentTrackerMeasurementGroups)
            {
                var recentTrackerMeasurementGroups = _recentTrackerMeasurementGroups
                    .ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<MeasurementGroup>)kvp.Value.ToImmutableArray());

                _recentTrackerMeasurementGroups.Clear();
                return recentTrackerMeasurementGroups;
            }
        }, cancellationToken);
    }
}