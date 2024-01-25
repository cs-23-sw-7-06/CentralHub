using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Measurement;
using CentralHub.Api.Model.Responses.Measurement;
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
        var registeredTrackers = (await trackerRepository.GetRegisteredTrackersAsync(token))
            .ToImmutableArray();
        var unregisteredTrackers = await trackerRepository.GetUnregisteredTrackersAsync(token);

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
        var aggregatedMeasurements = (await measurementRepository.GetAggregatedMeasurementsAsync(roomId, cancellationToken))
            .ToImmutableArray();

        if (!aggregatedMeasurements.Any())
        {
            return GetLatestOccupancyResponse.CreateUnsuccessful();
        }

        var aggregatedMeasurement = aggregatedMeasurements.Last();

        var occupancy = await _occupancySettings.Lock(
            os => (int)Math.Round(
                aggregatedMeasurement.BluetoothCount / os.BluetoothDevicesPerPerson +
                aggregatedMeasurement.WifiCount / os.WifiDevicesPerPerson),
            cancellationToken);


        return GetLatestOccupancyResponse.CreateSuccessful(occupancy);
    }

    [HttpGet("occupancy/percentage")]
    public async Task<float> GetEstimatedOccupancy(int roomId, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetRoomByIdAsync(roomId, cancellationToken);
        var aggregatedMeasurements = (await measurementRepository.GetAggregatedMeasurementsAsync(roomId, cancellationToken))
            .ToImmutableArray();

        if (room == null)
        {
            return 0f;
        }

        if (!aggregatedMeasurements.Any())
        {
            return 0f;
        }

        var aggregatedMeasurement = aggregatedMeasurements.Last();

        var occupancy = await _occupancySettings.Lock(
            os => (int)Math.Round(
                aggregatedMeasurement.BluetoothCount / os.BluetoothDevicesPerPerson +
                aggregatedMeasurement.WifiCount / os.WifiDevicesPerPerson),
            cancellationToken);

        if (occupancy == 0) 
        {
            return 0f;
        }

        return occupancy/room.Capacity;
    }

    [HttpPut("settings/set")]
    public async Task<SetSettingsResponse> SetSettings(
        SetSettingsRequest setSettingsRequest,
        CancellationToken cancellationToken)
    {
        if (setSettingsRequest.BluetoothDevicesPerPerson < 0 || setSettingsRequest.WifiDevicesPerPerson < 0)
        {
            return SetSettingsResponse.CreateUnsuccessful();
        }

        await _occupancySettings.Lock(os =>
        {
            os.BluetoothDevicesPerPerson = setSettingsRequest.BluetoothDevicesPerPerson;
            os.WifiDevicesPerPerson = setSettingsRequest.WifiDevicesPerPerson;
        }, cancellationToken);

        return SetSettingsResponse.CreateSuccessful();
    }

    private static IReadOnlyList<AggregatedMeasurements> CreateMeasurements(IEnumerable<AggregatedMeasurementDto> aggregatedMeasurements)
    {
        return aggregatedMeasurements.Select(am => new AggregatedMeasurements(
            am.AggregatedMeasurementDtoId,
            am.StartTime,
            am.EndTime,
            am.BluetoothCount,
            am.WifiCount)
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
