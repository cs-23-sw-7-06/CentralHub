using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class LocalizationControllerTests
{
    private LocalizationService _localizationService;
    private LocalizationController _localizationController;

    [SetUp]
    public void Setup()
    {
        _localizationService = new LocalizationService();
        _localizationController = new LocalizationController(_localizationService);
    }

    [Test]
    public async Task TestAddMeasurements()
    {
        var measurements = new Measurement[] {
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 20)

        };

        var addMeasurementsRequest = new AddMeasurementsRequest(0, measurements);
        await _localizationController.AddMeasurements(addMeasurementsRequest, default);

        Assert.That(_localizationService.Measurements, Has.Count.EqualTo(measurements.Length));
        Assert.That(_localizationService.Measurements, Does.Contain(measurements[0]));
        Assert.That(_localizationService.Measurements, Does.Contain(measurements[1]));
    }

    private sealed class LocalizationService : ILocalizationService
    {
        public List<Measurement> Measurements { get; } = new List<Measurement>();

        public void AddMeasurements(int id, IReadOnlyCollection<Measurement> measurements)
        {
            if (id != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "id is not 0");
            }

            Measurements.AddRange(measurements);
        }
    }
}