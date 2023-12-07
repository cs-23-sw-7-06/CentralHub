using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Model.Responses.AggregatedMeasurements;
using CentralHub.Api.Model.Responses.Measurements;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/measurements")]
public sealed class MeasurementController(
        IRoomRepository roomRepository,
        ITrackerRepository trackerRepository,
        IMeasurementRepository measurementRepository)
    : ControllerBase
{
    [HttpPost("add")]
    public async Task<AddMeasurementsResponse> AddMeasurements(AddMeasurementsRequest addMeasurementsRequest, CancellationToken token)
    {
        var registeredTrackers = (await trackerRepository.GetRegisteredTrackers(token))
            .ToImmutableArray();
        var unregisteredTrackers = await trackerRepository.GetUnregisteredTrackers(token);

        var tracker = registeredTrackers.SingleOrDefault(t => t.TrackerDtoId == addMeasurementsRequest.TrackerId);

        if (tracker == null)
        {
            return AddMeasurementsResponse.CreateUnsuccessful();
        }

        await measurementRepository.AddMeasurementsAsync(
            tracker.RoomDtoId,
            addMeasurementsRequest.Measurements
            .Where(m => !registeredTrackers.Any(t => t.WifiMacAddress == m.MacAddress || t.BluetoothMacAddress == m.MacAddress))
            .Where(m => !unregisteredTrackers.Any(t => t.WifiMacAddress == m.MacAddress || t.BluetoothMacAddress == m.MacAddress))
            .ToImmutableArray(),
            token);
        return AddMeasurementsResponse.CreateSuccessful();
    }

    [HttpGet("all")]
    public async Task<GetAggregatedMeasurementsResponse> GetAggregateMeasurements(int roomId, CancellationToken token)
    {
        var possibleRoom = await roomRepository.GetRoomByIdAsync(roomId, token);
        if (possibleRoom == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        var aggregatedMeasurements = (await measurementRepository.GetAggregatedMeasurementsAsync(roomId, token))
            .ToImmutableArray();

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful(CreateMeasurements(aggregatedMeasurements));
    }

    [HttpGet("range")]
    public async Task<GetAggregatedMeasurementsResponse> GetAggregateMeasurements(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken token)
    {
        var possibleRoom = await roomRepository.GetRoomByIdAsync(roomId, token);
        if (possibleRoom == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        var aggregatedMeasurements = (await measurementRepository.GetAggregatedMeasurementsAsync(roomId, timeStart, timeEnd, token))
            .ToImmutableArray();

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful(CreateMeasurements(aggregatedMeasurements));
    }

    [HttpGet("first")]
    public async Task<GetFirstAggregatedMeasurementsDateTimeResponse> GetFirstAggregatedMeasurementDateTime(int roomId,
        CancellationToken cancellationToken)
    {
        var possibleFirstDateTime =
            await measurementRepository.GetFirstAggregatedMeasurementsDateTimeAsync(roomId, cancellationToken);

        if (possibleFirstDateTime == null)
        {
            return GetFirstAggregatedMeasurementsDateTimeResponse.CreateUnsuccessful();
        }

        return GetFirstAggregatedMeasurementsDateTimeResponse.CreateSuccessful(possibleFirstDateTime.Value);
    }

    private static IReadOnlyList<AggregatedMeasurements> CreateMeasurements(IReadOnlyCollection<AggregatedMeasurementDto> aggregatedMeasurements)
    {
        return aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.BluetoothCount,
            am.WifiCount)
            ).ToImmutableArray();
    }
}
