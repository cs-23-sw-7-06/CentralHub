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
            _aggregatedMeasurementRepository);
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
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)
        };

        var addMeasurementsRequest = new AddMeasurementsRequest(_trackerId, measurements);
        await _measurementController.AddMeasurements(addMeasurementsRequest, default);
        var addedMeasurements = (await _aggregatedMeasurementRepository.GetTrackerMeasurementGroupsAsync(default))[_trackerId][0].Measurements;

        Assert.That(addedMeasurements, Has.Count.EqualTo(measurements.Length));
        Assert.That(addedMeasurements, Does.Contain(measurements[0]));
        Assert.That(addedMeasurements, Does.Contain(measurements[1]));
    }
}