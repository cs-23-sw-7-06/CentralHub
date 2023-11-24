using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentralHub.Api.Tests;

public class LocalizationControllerTests
{
    private MeasurementController _measurementController;
    private IAggregatedMeasurementRepository _aggregatorRepository;

    [SetUp]
    public void Setup()
    {
        _aggregatorRepository = new MockAggregatedMeasurementRepository();
        _measurementController = new MeasurementController(
            NullLogger<MeasurementController>.Instance,
            _aggregatorRepository);
    }

    [Test]
    public async Task TestAddMeasurements()
    {
        var measurements = new Measurement[] {
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)

        };

        var addMeasurementsRequest = new AddMeasurementsRequest(0, measurements);
        await _measurementController.AddMeasurements(addMeasurementsRequest, default);
        var addedMeasurements = (await _aggregatorRepository.GetTrackerMeasurementGroupsAsync(default))[0][0].Measurements;

        Assert.That(addedMeasurements, Has.Count.EqualTo(measurements.Length));
        Assert.That(addedMeasurements, Does.Contain(measurements[0]));
        Assert.That(addedMeasurements, Does.Contain(measurements[1]));
    }
}