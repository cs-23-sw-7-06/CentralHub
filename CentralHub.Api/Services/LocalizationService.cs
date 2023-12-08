using System.Collections.Immutable;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Threading;

namespace CentralHub.Api.Services;

public class LocalizationService(
    ILogger<LocalizationService> logger,
    IMeasurementRepository aggregatorRepository,
    IRoomRepository roomRepository)
    : IScopedProcessingService
{
    private static readonly TimeSpan SleepTime = TimeSpan.FromMinutes(5);

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AggregateMeasurementsAsync(stoppingToken);
            await Task.Delay(SleepTime, stoppingToken);
        }
    }

    public async Task AggregateMeasurementsAsync(CancellationToken stoppingToken)
    {
        var roomMeasurementGroups = await aggregatorRepository.GetRoomMeasurementGroupsAsync(stoppingToken);
        var allRooms = await roomRepository.GetRoomsAsync(stoppingToken);
        using var measuredRoomsMutex = new CancellableMutex<List<RoomDto>>(new List<RoomDto>());

        await Parallel.ForEachAsync(
            roomMeasurementGroups,
            stoppingToken,
            async (roomMeasurementGroup, cancellationToken) =>
        {
            var room = allRooms.SingleOrDefault(r => r.RoomDtoId == roomMeasurementGroup.Key);

            if (room == null)
            {
                logger.LogWarning("Room with id {RoomId} was not found.", roomMeasurementGroup.Key);
                return;
            }

            await measuredRoomsMutex.Lock(m => m.Add(room), cancellationToken);

            await aggregatorRepository.AddAggregatedMeasurementAsync(
                CreateAggregatedMeasurement(room, roomMeasurementGroup.Value),
                cancellationToken);
        });

        await measuredRoomsMutex.Lock(async m =>
        {
            foreach (var room in allRooms.Where(r => !m.Contains(r)))
            {
                await aggregatorRepository.AddAggregatedMeasurementAsync(
                    CreateAggregatedMeasurement(room, new List<MeasurementGroup>()),
                    stoppingToken);
            }
        },
        stoppingToken);
    }

    private static AggregatedMeasurementDto CreateAggregatedMeasurement(RoomDto room, IReadOnlyCollection<MeasurementGroup> measurementGroups)
    {
        var measurements = measurementGroups.SelectMany(mg => mg.Measurements);
        var filteredMeasurements = FilterMeasurements(measurements).ToImmutableArray();
        var bluetoothCount = filteredMeasurements.Count(m => m.Type == Measurement.Protocol.Bluetooth);
        var wifiCount = filteredMeasurements.Count(m => m.Type == Measurement.Protocol.Wifi);
        var now = DateTime.UtcNow;

        var startMeasurementTime = measurementGroups.Count != 0 ? measurementGroups.Min(mg => mg.Timestamp) : now - SleepTime;
        var endMeasurementTime = measurementGroups.Count != 0 ? measurementGroups.Max(mg => mg.Timestamp) : now;

        var roomDtoId = room.RoomDtoId;

        var roomDto = room;

        return new AggregatedMeasurementDto
        {
            StartTime = startMeasurementTime,
            EndTime = endMeasurementTime,
            BluetoothCount = bluetoothCount,
            WifiCount = wifiCount,
            RoomDtoId = roomDtoId,
            RoomDto = roomDto
        };
    }

    /// <summary>
    /// Filter out measurements of the same mac Address, the latest measurement is kept.
    /// </summary>
    /// <param name="measurements">An enumerable collection of the measurements</param>
    /// <returns>An enumerable collection of measurements with only unique macAddresses.</returns>
    private static IEnumerable<Measurement> FilterMeasurements(IEnumerable<Measurement> measurements)
    {
        var filteredMeasurements = new Dictionary<FilterCriteria, Measurement>();
        foreach (var (filterCriteria, measurement) in measurements.Select(m => (new FilterCriteria(m.MacAddress, m.Type), m)))
        {
            if (!filteredMeasurements.TryAdd(filterCriteria, measurement))
            {
                filteredMeasurements[filterCriteria] = measurement;
            }
        }

        return filteredMeasurements.Values.ToImmutableArray();
    }

    private readonly struct FilterCriteria(string macAddress, Measurement.Protocol protocol)
    {
        public string MacAddress { get; } = macAddress;
        public Measurement.Protocol Protocol { get; } = protocol;

        public override int GetHashCode()
        {
            return HashCode.Combine(MacAddress, Protocol);
        }
    }
}