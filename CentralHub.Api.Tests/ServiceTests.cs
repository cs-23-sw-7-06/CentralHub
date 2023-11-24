
using CentralHub.Api.Controllers;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

class ServiceTests
{
    private TestLocalizationService _localizationService;
    [SetUp]
    public void Setup()
    {
        _localizationService = new TestLocalizationService
        {
            MaxAge = TimeSpan.FromSeconds(1) //set removal period to 1 seconds
        };
    }

    [Test]
    public void TestRemoveMeasurement()
    {
        _localizationService.AddMeasurements(1, new List<Measurement>(){
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Wifi, 1),
            new Measurement("11:22:33:44:55:66", Measurement.Protocol.Bluetooth, 10)
        });
        Assert.That(_localizationService.getMeasurements(1).Count == 1);
        Thread.Sleep((int)(_localizationService.MaxAge.TotalMilliseconds / 4)); // wait period/4 seconds
        _localizationService.AddMeasurements(1, new List<Measurement>(){
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Wifi, 1),
            new Measurement("aa:bb:cc:dd:ee:ff", Measurement.Protocol.Bluetooth, 10)
        });
        Assert.That(_localizationService.getMeasurements(1).Count == 2);
        Thread.Sleep((int)(_localizationService.MaxAge.TotalMilliseconds - (int)(_localizationService.MaxAge.TotalMilliseconds / 4))); // wait period-(period/4) seconds
        Assert.That(_localizationService.getMeasurements(1).Count == 1);
        Thread.Sleep((int)(_localizationService.MaxAge.TotalMilliseconds / 2)); // wait period/2 seconds
        Assert.That(_localizationService.getMeasurements(1).Count == 0);
    }
}

class TestLocalizationService : LocalizationService
{
    public List<MeasurementGroup> getMeasurements(int id)
    {
        return _measurements[id];
    }
}