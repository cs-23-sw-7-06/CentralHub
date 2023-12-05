using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Model.Responses.AggregatedMeasurements;
using CentralHub.Api.Model.Responses.Measurements;
using CentralHub.Api.Services;
using CentralHub.Api.Threading;
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
    private static CancellableMutex<OccupancySettings> _occupancySettings =
        new CancellableMutex<OccupancySettings>(
            new OccupancySettings(1.5f, 2.4f));

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

    [HttpGet("occupancy/latest")]
    public async Task<GetLatestOccupancyResponse> GetLatestEstimatedOccupancy(int roomId, CancellationToken cancellationToken)
    {
        var aggregatedMeasurements = (await measurementRepository.GetAggregatedMeasurementsAsync(roomId, cancellationToken)).Last();

        if (aggregatedMeasurements == null)
        {
            return GetLatestOccupancyResponse.CreateUnsuccessful();
        }

        var occupancy = await _occupancySettings.Lock(os =>
        {
            return (int)Math.Round(
                aggregatedMeasurements.BluetoothCount / os.BluetoothDevicesPerPerson +
                aggregatedMeasurements.WifiCount / os.WifiDevicesPerPerson);
        }, cancellationToken);


        return GetLatestOccupancyResponse.CreateSuccessful(occupancy);
    }

    [HttpPost("settings/set")]
    public async Task<SetDevicesPerPersonResponse> SetDevicesPerPerson(
        float bluetoothDevicesPerPerson,
        float wifiDevicesPerPerson,
        CancellationToken cancellationToken)
    {
        if (bluetoothDevicesPerPerson < 0 || wifiDevicesPerPerson < 0)
        {
            return SetDevicesPerPersonResponse.CreateUnsuccessful();
        }

        await _occupancySettings.Lock(os =>
        {
            os.BluetoothDevicesPerPerson = bluetoothDevicesPerPerson;
            os.WifiDevicesPerPerson = wifiDevicesPerPerson;
        }, cancellationToken);

        return SetDevicesPerPersonResponse
            .CreateSuccessful(
                bluetoothDevicesPerPerson, wifiDevicesPerPerson);
    }

    private static IReadOnlyList<AggregatedMeasurements> CreateMeasurements(IEnumerable<AggregatedMeasurementDto> aggregatedMeasurements)
    {
        var recentAggregatedMeasurements = aggregatedMeasurements
            .Where(am => am.EndTime > (DateTime.UtcNow - TimeSpan.FromDays(1)))
            .ToImmutableArray();

        var bluetoothCalibrationNumber = recentAggregatedMeasurements.Min(am => am.BluetoothCount);
        var wifiCalibrationNumber = recentAggregatedMeasurements.Min(am => am.WifiCount);

        return aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.BluetoothCount - bluetoothCalibrationNumber,
            am.WifiCount - wifiCalibrationNumber)
            ).ToImmutableArray();
    }

    private struct OccupancySettings
    {
        public OccupancySettings(float bluetoothDevicesPerPerson, float wifiDevicesPerPerson)
        {
            BluetoothDevicesPerPerson = bluetoothDevicesPerPerson;
            WifiDevicesPerPerson = wifiDevicesPerPerson;
        }
        public float BluetoothDevicesPerPerson { get; set; }
        public float WifiDevicesPerPerson { get; set; }
    }
}
