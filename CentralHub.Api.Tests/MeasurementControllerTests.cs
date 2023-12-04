using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentralHub.Api.Tests;

public class MeasurementControllerTests
{
    private MeasurementController _measurementController;
    private IRoomRepository _roomRepository;
    private ITrackerRepository _trackerRepository;
    private IMeasurementRepository _aggregatedMeasurementRepository;

    private LocalizationService _localizationService;

    private int _roomId = 0;

    private int _trackerId = 0;

    [SetUp]
    public void Setup()
    {
        var roomDto = new RoomDto()
        {
            Name = "Test Room",
            Description = "Test Room",
        };

        _roomRepository = new MockRoomRepository();
        _roomId = _roomRepository.AddRoomAsync(roomDto, default).GetAwaiter().GetResult();

        var trackerDto = new TrackerDto()
        {
            Name = "Test Tracker",
            Description = "Test Tracker",
            BluetoothMacAddress = "11:22:33:44:55:66",
            WifiMacAddress = "aa:bb:cc:dd:ee:ff",
            RoomDtoId = roomDto.RoomDtoId,
            RoomDto = roomDto
        };
        _trackerRepository = new MockTrackerRepository(roomDto);
        _trackerId = _trackerRepository.AddTrackerAsync(trackerDto, default).GetAwaiter().GetResult();

        _aggregatedMeasurementRepository = new MockAggregatedMeasurementRepository();
        _measurementController = new MeasurementController(
            NullLogger<MeasurementController>.Instance,
            _aggregatedMeasurementRepository,
            _trackerRepository);

        _localizationService = new LocalizationService(
            NullLogger<LocalizationService>.Instance,
            _aggregatedMeasurementRepository,
            _roomRepository
        );
    }

    [TearDown]
    public void TearDown()
    {
        var aggregatedMeasurements = _aggregatedMeasurementRepository.GetAggregatedMeasurementsAsync(_roomId, default).GetAwaiter().GetResult();
        foreach (var measurement in aggregatedMeasurements)
        {
            _aggregatedMeasurementRepository.RemoveAggregatedMeasurementAsync(measurement, default).GetAwaiter().GetResult();
        }
    }

    [Test]
    public async Task TestAddMeasurements()
    {
        var measurements = new Measurement[] {
            new Measurement("22:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("bb:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest = new AddMeasurementsRequest(_trackerId, measurements);
        await _measurementController.AddMeasurements(addMeasurementsRequest, default);
        var addedMeasurements = (await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default))[_roomId][0].Measurements;

        Assert.That(addedMeasurements, Has.Count.EqualTo(measurements.Length));
        Assert.That(addedMeasurements, Does.Contain(measurements[0]));
        Assert.That(addedMeasurements, Does.Contain(measurements[1]));
    }

    [Test]
    public async Task TrackersFilteredFromMeasurements()
    {
        var measurements = new Measurement[] {
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest = new AddMeasurementsRequest(_trackerId, measurements);
        await _measurementController.AddMeasurements(addMeasurementsRequest, default);
        var addedMeasurements = (await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default))[_roomId][0].Measurements;

        Assert.That(addedMeasurements, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task CalibrateMeasurements()
    {
        var measurements = new Measurement[] {
            new Measurement("12:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("ab:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest = new AddMeasurementsRequest(_trackerId, measurements);
        await _measurementController.AddMeasurements(addMeasurementsRequest, default);

        var measurements2 = new Measurement[] {
            new Measurement("13:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("ac:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20),
            new Measurement("14:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("ab:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest2 = new AddMeasurementsRequest(_trackerId, measurements2);
        await _measurementController.AddMeasurements(addMeasurementsRequest2, default);
        await _localizationService.AggregateMeasurementsAsync(default);

        var measurements3 = new Measurement[] {
            new Measurement("13:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("ac:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20),
            new Measurement("14:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("ab:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest3 = new AddMeasurementsRequest(_trackerId, measurements3);
        await _measurementController.AddMeasurements(addMeasurementsRequest3, default);
        await _localizationService.AggregateMeasurementsAsync(default);

        var lastAggregatedMeasurements = (await _measurementController.GetAggregateMeasurements(_roomId, default)).AggregatedMeasurements.Last();

        Assert.That(lastAggregatedMeasurements.BluetoothMedian, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.WifiMedian, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.BluetoothMean, Is.EqualTo(1f));
        Assert.That(lastAggregatedMeasurements.WifiMean, Is.EqualTo(1f));
        Assert.That(lastAggregatedMeasurements.BluetoothMax, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.WifiMax, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.BluetoothMin, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.WifiMin, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.TotalBluetoothDeviceCount, Is.EqualTo(1));
        Assert.That(lastAggregatedMeasurements.TotalWifiDeviceCount, Is.EqualTo(1));
    }
}