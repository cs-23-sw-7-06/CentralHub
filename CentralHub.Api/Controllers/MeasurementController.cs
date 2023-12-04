using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Model.Responses.AggregatedMeasurements;
using CentralHub.Api.Model.Responses.Measurements;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/measurements")]
public sealed class MeasurementController : ControllerBase
{
    private readonly ILogger<MeasurementController> _logger;

    private readonly IMeasurementRepository _measurementRepository;

    private readonly ITrackerRepository _trackerRepository;

    public MeasurementController(
        ILogger<MeasurementController> logger,
        IMeasurementRepository measurementRepository,
        ITrackerRepository trackerRepository)
    {
        _logger = logger;
        _measurementRepository = measurementRepository;
        _trackerRepository = trackerRepository;
    }

    [HttpPost("add")]
    public async Task<AddMeasurementsResponse> AddMeasurements(AddMeasurementsRequest addMeasurementsRequest, CancellationToken token)
    {
        var registeredTrackers = await _trackerRepository.GetRegisteredTrackers(token);
        var unregisteredTrackers = await _trackerRepository.GetUnregisteredTrackers(token);

        var tracker = registeredTrackers.SingleOrDefault(t => t.TrackerDtoId == addMeasurementsRequest.TrackerId);

        if (tracker == null)
        {
            return AddMeasurementsResponse.CreateUnsuccessful();
        }

        await _measurementRepository.AddMeasurementsAsync(
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
        var aggregatedMeasurements = await _measurementRepository.GetAggregatedMeasurementsAsync(roomId, token);

        if (aggregatedMeasurements == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful(CreateMeasurements(aggregatedMeasurements));
    }

    [HttpGet("range")]
    public async Task<GetAggregatedMeasurementsResponse> GetAggregateMeasurements(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken token)
    {
        var aggregatedMeasurements = await _measurementRepository.GetAggregatedMeasurementsAsync(roomId, timeStart, timeEnd, token);

        if (aggregatedMeasurements == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful(CreateMeasurements(aggregatedMeasurements));
    }

    [HttpGet("first")]
    public async Task<GetFirstAggregatedMeasurementsDateTimeResponse> GetFirstAggregatedMeasurementDateTime(int roomId,
        CancellationToken cancellationToken)
    {
        var possibleFirstDateTime =
            await _measurementRepository.GetFirstAggregatedMeasurementsDateTimeAsync(roomId, cancellationToken);

        if (possibleFirstDateTime == null)
        {
            return GetFirstAggregatedMeasurementsDateTimeResponse.CreateUnsuccessful();
        }

        return GetFirstAggregatedMeasurementsDateTimeResponse.CreateSuccessful(possibleFirstDateTime.Value);
    }

    private static IReadOnlyList<AggregatedMeasurements> CreateMeasurements(IEnumerable<AggregatedMeasurementDto> aggregatedMeasurements)
    {
        var recentAggregatedMeasurements = aggregatedMeasurements.Where(am => am.EndTime > (DateTime.UtcNow - TimeSpan.FromDays(1)));
        var wifiCalibrationNumber = recentAggregatedMeasurements.Min(am => am.WifiMinDeviceCount);
        var bluetoothCalibrationNumber = recentAggregatedMeasurements.Min(am => am.BluetoothMinDeviceCount);

        return aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.MeasurementGroupCount,
            am.BluetoothMedianDeviceCount - bluetoothCalibrationNumber,
            am.BluetoothMeanDeviceCount - bluetoothCalibrationNumber,
            am.BluetoothMaxDeviceCount - bluetoothCalibrationNumber,
            am.BluetoothMinDeviceCount - bluetoothCalibrationNumber,
            am.TotalBluetoothDeviceCount - bluetoothCalibrationNumber,
            am.WifiMedianDeviceCount - wifiCalibrationNumber,
            am.WifiMeanDeviceCount - wifiCalibrationNumber,
            am.WifiMaxDeviceCount - wifiCalibrationNumber,
            am.WifiMinDeviceCount - wifiCalibrationNumber,
            am.TotalWifiDeviceCount - wifiCalibrationNumber)
            ).ToImmutableArray();
    }
}
