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

    public MeasurementController(
        ILogger<MeasurementController> logger,
        IMeasurementRepository aggregatorRepository)
    {
        _logger = logger;
        _aggregatorRepository = aggregatorRepository;
    }

    [HttpPost("add")]
    public async Task AddMeasurements(AddMeasurementsRequest addMeasurementsRequest, CancellationToken token)
    {
        await _aggregatorRepository.AddMeasurementsAsync(
            addMeasurementsRequest.TrackerId,
            addMeasurementsRequest.Measurements,
            token);
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
