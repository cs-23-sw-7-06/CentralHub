using System.Collections.Immutable;
using System.Linq;
using App.ScopedService;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationService : IScopedProcessingService
{
    private static readonly TimeSpan _sleepTime = TimeSpan.FromMinutes(2);
    private readonly ILogger<LocalizationService> _logger;

    private readonly IMeasurementRepository _aggregatorRepository;

    private readonly IRoomRepository _roomRepository;

    private readonly ITrackerRepository _trackerRepository;

    public LocalizationService(
        ILogger<LocalizationService> logger,
        IMeasurementRepository aggregatorRepository,
        IRoomRepository roomRepository,
        ITrackerRepository trackerRepository)
    {
        _logger = logger;
        _aggregatorRepository = aggregatorRepository;
        _roomRepository = roomRepository;
        _trackerRepository = trackerRepository;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AggregateMeasurementsAsync(stoppingToken);
            await Task.Delay(_sleepTime, stoppingToken);
        }
    }

    public async Task AggregateMeasurementsAsync(CancellationToken stoppingToken)
    {
        var trackerMeasurementGroups = await _aggregatorRepository.GetTrackerMeasurementGroupsAsync(stoppingToken);

        foreach (var (trackerId, measurementGroups) in trackerMeasurementGroups)
        {
            var tracker = await _trackerRepository.GetTrackerByIdAsync(
                trackerId,
                stoppingToken);

            if (tracker == null)
            {
                _logger.LogWarning("Tracker with id {TrackerId} was not found.", trackerId);
                continue;
            }

            var room = await _roomRepository.GetRoomByIdAsync(
                tracker.RoomDtoId,
                stoppingToken);

            if (room == null)
            {
                _logger.LogWarning("Room with id {RoomId} was not found.", tracker.RoomDtoId);
                continue;
            }

            await _aggregatorRepository.AddAggregatedMeasurementAsync(
                CreateAggregatedMeasurement(room, measurementGroups),
                stoppingToken);
        }
    }

    private static AggregatedMeasurementDto CreateAggregatedMeasurement(RoomDto room, IReadOnlyCollection<MeasurementGroup> measurementGroups)
    {
        var measurements = measurementGroups.SelectMany(mg => mg.Measurements);
        var bluetooth = measurements.Where(m => m.Type == Measurement.Protocol.Bluetooth);
        var wifi = measurements.Where(m => m.Type == Measurement.Protocol.Wifi);

        var startMeasurementTime = measurementGroups.Min(mg => mg.Timestamp);
        var endMeasurementTime = measurementGroups.Max(mg => mg.Timestamp);

        var filteredMeasurementsGroups = measurementGroups.Select(mg => FilterMeasurements(mg.Measurements)).ToImmutableArray();
        var numBluetoothPerGroup = filteredMeasurementsGroups.Select(m => m.Count(m => m.Type == Measurement.Protocol.Bluetooth)).ToImmutableArray();
        var numWifiPerGroup = filteredMeasurementsGroups.Select(m => m.Count(m => m.Type == Measurement.Protocol.Wifi)).ToImmutableArray();


        var measurementCount = measurementGroups.Count;

        var bluetoothMax = MaxDevices(numBluetoothPerGroup);

        var bluetoothMedian = MedianDevices(numBluetoothPerGroup);

        var bluetoothMean = MeanDevices(numBluetoothPerGroup);

        var bluetoothMin = MinDevices(numBluetoothPerGroup);

        var bluetoothCount = FilterMeasurements(bluetooth).Count();

        var wifiMax = MaxDevices(numWifiPerGroup);

        var wifiMedian = MedianDevices(numWifiPerGroup);

        var wifiMean = MeanDevices(numWifiPerGroup);

        var wifiMin = MinDevices(numWifiPerGroup);

        var wifiCount = FilterMeasurements(wifi).Count();

        var roomDtoId = room.RoomDtoId;

        var roomDto = room;

        return new AggregatedMeasurementDto
        {
            StartTime = startMeasurementTime,
            EndTime = endMeasurementTime,
            MeasurementGroupCount = measurementCount,
            BluetoothMaxDeviceCount = bluetoothMax,
            BluetoothMedianDeviceCount = bluetoothMedian,
            BluetoothMeanDeviceCount = bluetoothMean,
            BluetoothMinDeviceCount = bluetoothMin,
            TotalBluetoothDeviceCount = bluetoothCount,
            WifiMaxDeviceCount = wifiMax,
            WifiMedianDeviceCount = wifiMedian,
            WifiMeanDeviceCount = wifiMean,
            WifiMinDeviceCount = wifiMin,
            TotalWifiDeviceCount = wifiCount,
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

        return filteredMeasurements.Values.Where(m => m.Rssi >= -75);
    }

    private static int MaxDevices(IEnumerable<int> numMatchingProtocolPerGroup)
    {
        if (!numMatchingProtocolPerGroup.Any())
        {
            return 0;
        }

        return numMatchingProtocolPerGroup.Max();
    }

    private static int MinDevices(IEnumerable<int> numMatchingProtocolPerGroup)
    {
        if (!numMatchingProtocolPerGroup.Any())
        {
            return 0;
        }

        return numMatchingProtocolPerGroup.Min();
    }

    private static int MedianDevices(IEnumerable<int> numMatchingProtocolPerGroup)
    {
        if (!numMatchingProtocolPerGroup.Any())
        {
            return 0;
        }

        return numMatchingProtocolPerGroup
            .Order()
            .ElementAt((int)Math.Floor(numMatchingProtocolPerGroup.Count() / 2f));
    }

    private static double MeanDevices(IEnumerable<int> numMatchingProtocolPerGroup)
    {
        if (!numMatchingProtocolPerGroup.Any())
        {
            return 0;
        }

        return numMatchingProtocolPerGroup.Average();
    }
    private readonly struct FilterCriteria
    {
        public string MacAddress { get; }
        public Measurement.Protocol Protocol { get; }

        public FilterCriteria(string macAddress, Measurement.Protocol protocol)
        {
            MacAddress = macAddress;
            Protocol = protocol;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MacAddress, Protocol);
        }
    }
}