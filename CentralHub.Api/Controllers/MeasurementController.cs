using System.Collections.Immutable;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Model.Responses.AggregatedMeasurements;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/measurements")]
public sealed class MeasurementController : ControllerBase
{
    private readonly ILogger<MeasurementController> _logger;

    private readonly IMeasurementRepository _aggregatorRepository;

    private readonly ITrackerRepository _trackerRepository;

    public MeasurementController(
        ILogger<MeasurementController> logger,
        IMeasurementRepository aggregatorRepository,
        ITrackerRepository trackerRepository)
    {
        _logger = logger;
        _aggregatorRepository = aggregatorRepository;
        _trackerRepository = trackerRepository;
    }

    [HttpPost("add")]
    public async Task<AddMeasurementResponse> AddMeasurements(AddMeasurementsRequest addMeasurementsRequest, CancellationToken token)
    {
        var registeredTrackers = await _trackerRepository.GetRegisteredTrackers(token);
        var unregisteredTrackers = await _trackerRepository.GetUnregisteredTrackers(token);

        if (registeredTrackers.Any(t => t.TrackerDtoId == addMeasurementsRequest.TrackerId))
        {
            await _aggregatorRepository.AddMeasurementsAsync(
                addMeasurementsRequest.TrackerId,
                addMeasurementsRequest.Measurements
                .Where(m => !registeredTrackers.Any(t => t.WifiMacAddress == m.MacAddress || t.BluetoothMacAddress == m.MacAddress))
                .Where(m => !unregisteredTrackers.Any(t => t.WifiMacAddress == m.MacAddress || t.BluetoothMacAddress == m.MacAddress))
                .ToImmutableArray(),
                token);
            return AddMeasurementResponse.CreateSuccessful();
        }

        return AddMeasurementResponse.CreateUnsuccessful();

    }

    [HttpGet("all")]
    public async Task<GetAggregatedMeasurementsResponse> GetAggregateMeasurements(int roomId, CancellationToken token)
    {
        var aggregatedMeasurements = await _aggregatorRepository.GetAggregatedMeasurementsAsync(roomId, token);

        if (aggregatedMeasurements == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse.CreateSuccessful(
            aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.MeasurementGroupCount,
            am.BluetoothMedianDeviceCount,
            am.BluetoothMeanDeviceCount,
            am.BluetoothMaxDeviceCount,
            am.BluetoothMinDeviceCount,
            am.TotalBluetoothDeviceCount,
            am.WifiMedianDeviceCount,
            am.WifiMeanDeviceCount,
            am.WifiMaxDeviceCount,
            am.WifiMinDeviceCount,
            am.TotalWifiDeviceCount)).ToImmutableArray());
    }

    [HttpGet("range")]
    public async Task<GetAggregatedMeasurementsResponse> GetAggregateMeasurements(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken token)
    {
        var aggregatedMeasurements = await GetAggregateMeasurements(roomId, token);

        if (!aggregatedMeasurements.Success)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful(aggregatedMeasurements.AggregatedMeasurements!
            .Where(am => am.StartTime >= timeStart && am.EndTime <= timeEnd)
            .ToImmutableArray());
    }
}
