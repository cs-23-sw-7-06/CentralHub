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

    private readonly IAggregatedMeasurementRepository _aggregatorRepository;

    public MeasurementController(
        ILogger<MeasurementController> logger,
        IAggregatedMeasurementRepository aggregatorRepository)
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

    [HttpGet("get")]
    public async Task<GetAggregatedMeasurementsResponse> GetMeasurements(int roomId, CancellationToken token)
    {
        var aggregatedMeasurements = await _aggregatorRepository.GetMeasurementsAsync(roomId, token);

        if (aggregatedMeasurements == null)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse.CreateSuccessful(
            (IReadOnlyCollection<AggregatedMeasurements>)aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.MeasurementGroupCount,
            am.BluetoothMedian,
            am.BluetoothMean,
            am.BluetoothMax,
            am.BluetoothMin,
            am.TotalBluetoothDeviceCount,
            am.WifiMedian,
            am.WifiMean,
            am.WifiMax,
            am.WifiMin,
            am.TotalWifiDeviceCount)));
    }

    [HttpGet("getperiod")]
    public async Task<GetAggregatedMeasurementsResponse> GetMeasurements(int roomId, DateTime timeStart, DateTime timeEnd, CancellationToken token)
    {
        var aggregatedMeasurements = await GetMeasurements(roomId, token);

        if (!aggregatedMeasurements.Success)
        {
            return GetAggregatedMeasurementsResponse.CreateUnsuccessful();
        }

        return GetAggregatedMeasurementsResponse
            .CreateSuccessful((IReadOnlyCollection<AggregatedMeasurements>)aggregatedMeasurements.AggregatedMeasurements!
            .Where(am => am.StartTime >= timeStart && am.EndTime <= timeEnd));
    }
}
