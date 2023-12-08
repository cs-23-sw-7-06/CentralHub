using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Measurement;
using CentralHub.Api.Model.Responses.Measurement;
using CentralHub.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentralHub.Api.Tests;

public class MeasurementControllerTests
{
    private MeasurementController _measurementController;
    private IRoomRepository _roomRepository;
    private ITrackerRepository _trackerRepository;
    private IMeasurementRepository _measurementRepository;

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

        _measurementRepository = new MockAggregatedMeasurementRepository();
        _measurementController = new MeasurementController(
            _roomRepository,
            _trackerRepository,
            _measurementRepository
            );

        _localizationService = new LocalizationService(
            NullLogger<LocalizationService>.Instance,
            _measurementRepository,
            _roomRepository
        );
    }

    [TearDown]
    public void TearDown()
    {
        var aggregatedMeasurements = _measurementRepository.GetAggregatedMeasurementsAsync(_roomId, default).GetAwaiter().GetResult();
        foreach (var measurement in aggregatedMeasurements)
        {
            _measurementRepository.RemoveAggregatedMeasurementAsync(measurement, default).GetAwaiter().GetResult();
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
        var addedMeasurements = (await _measurementRepository.GetRoomMeasurementGroupsAsync(default))[_roomId][0].Measurements;

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
        var addedMeasurements = (await _measurementRepository.GetRoomMeasurementGroupsAsync(default))[_roomId][0].Measurements;

        Assert.That(addedMeasurements, Has.Count.EqualTo(0));
    }
}