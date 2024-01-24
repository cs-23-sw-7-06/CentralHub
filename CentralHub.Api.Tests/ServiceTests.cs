
using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Responses.Measurement;
using CentralHub.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentralHub.Api.Tests;

class ServiceTests
{
    private IRoomRepository _roomRepository;
    private ITrackerRepository _trackerRepository;
    private IMeasurementRepository _aggregatedMeasurementRepository;

    private int _roomId = 0;

    private LocalizationService _localizationService;

    [SetUp]
    public void Setup()
    {
        var roomDto = new RoomDto()
        {
            Name = "Test Room",
            Description = "Test Room",
            Capacity = 10,
            NeighbouringRooms = [],
        };

        _roomRepository = new MockRoomRepository();
        _roomId = _roomRepository.AddRoomAsync(roomDto, default).GetAwaiter().GetResult();

        var trackerDto = new TrackerDto()
        {
            Name = "Test Tracker",
            Description = "Test Tracker",
            BluetoothMacAddress = "ff:22:33:44:55:66",
            WifiMacAddress = "ff:bb:cc:dd:ee:ff",
            RoomDtoId = roomDto.RoomDtoId,
            RoomDto = roomDto
        };
        _trackerRepository = new MockTrackerRepository(roomDto);
        _trackerRepository.AddTrackerAsync(trackerDto, default).GetAwaiter().GetResult();

        _aggregatedMeasurementRepository = new MockAggregatedMeasurementRepository();
        _localizationService = new LocalizationService(
            NullLogger<LocalizationService>.Instance,
            _aggregatedMeasurementRepository,
            _roomRepository);
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
    public async Task TestAggregateSingleGroupMeasurements()
    {
        await _aggregatedMeasurementRepository.AddMeasurementsAsync(_roomId, new List<Measurement>(){
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
        }, default);

        await _localizationService.AggregateMeasurementsAsync(default);
        Assert.That(await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default), Is.Empty);

        var aggregatedMeasurements = await _aggregatedMeasurementRepository.GetAggregatedMeasurementsAsync(_roomId, default);

        Assert.That(aggregatedMeasurements, Has.Exactly(1).Items);

        var aggregatedMeasurement = aggregatedMeasurements.First();

        Assert.That(aggregatedMeasurement.BluetoothCount, Is.EqualTo(1));
        Assert.That(aggregatedMeasurement.WifiCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TestAggregateMultipleGroupsOfMeasurements()
    {
        await _aggregatedMeasurementRepository.AddMeasurementsAsync(_roomId, new List<Measurement>(){
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
            new Measurement("21:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("21:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
        }, default);

        await _aggregatedMeasurementRepository.AddMeasurementsAsync(_roomId, new List<Measurement>(){
            new Measurement("31:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("41:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
            new Measurement("51:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("71:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, -30),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Bluetooth, -10),
        }, default);

        await _localizationService.AggregateMeasurementsAsync(default);
        Assert.That(await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default), Is.Empty);

        var aggregatedMeasurements = await _aggregatedMeasurementRepository.GetAggregatedMeasurementsAsync(_roomId, default);

        Assert.That(aggregatedMeasurements, Has.Exactly(1).Items);

        var aggregatedMeasurement = aggregatedMeasurements.First();

        Assert.That(aggregatedMeasurement.BluetoothCount, Is.EqualTo(5));
        Assert.That(aggregatedMeasurement.WifiCount, Is.EqualTo(5));
    }

    [Test]
    public async Task TestAggregateDuplicateMeasurements()
    {
        await _aggregatedMeasurementRepository.AddMeasurementsAsync(_roomId, new List<Measurement>(){
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Wifi, -40),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Wifi, -10),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, -40),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, -10),
        }, default);

        await _localizationService.AggregateMeasurementsAsync(default);
        Assert.That(await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default), Is.Empty);

        var aggregatedMeasurements = await _aggregatedMeasurementRepository.GetAggregatedMeasurementsAsync(_roomId, default);

        Assert.That(aggregatedMeasurements, Has.Exactly(1).Items);

        var aggregatedMeasurement = aggregatedMeasurements.First();

        Assert.That(aggregatedMeasurement.BluetoothCount, Is.EqualTo(1));
        Assert.That(aggregatedMeasurement.WifiCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TestNoMeasurements()
    {
        await _localizationService.AggregateMeasurementsAsync(default);
        Assert.That(await _aggregatedMeasurementRepository.GetRoomMeasurementGroupsAsync(default), Is.Empty);

        var aggregatedMeasurements = await _aggregatedMeasurementRepository.GetAggregatedMeasurementsAsync(_roomId, default);

        Assert.That(aggregatedMeasurements, Has.Exactly(1).Items);

        var aggregatedMeasurement = aggregatedMeasurements.First();

        Assert.That(aggregatedMeasurement.BluetoothCount, Is.EqualTo(0));
        Assert.That(aggregatedMeasurement.WifiCount, Is.EqualTo(0));
    }
}