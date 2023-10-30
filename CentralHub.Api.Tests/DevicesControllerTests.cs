using CentralHub.Api.Controllers;
using CentralHub.Api.Model;
using CentralHub.Api.Services;

namespace CentralHub.Api.Tests;

public class DevicesControllerTests
{
    private DevicesController _devicesController;

    [SetUp]
    public void Setup()
    {
        var devicesRepository = new DevicesRepository();
        _devicesController = new DevicesController(devicesRepository);
    }

    [Test]
    public async Task TestEmptyByDefault()
    {
        var devices = await _devicesController.Get();
        Assert.That(devices, Is.Empty);
    }

    [Test]
    public async Task TestAddDevice()
    {
        var device = new Device("Test Device", "AA:BB:CC:DD:EE:FF", DeviceType.WiFi);
        await _devicesController.Post(device);

        var devices = await _devicesController.Get();
        Assert.That(devices.Single(), Is.EqualTo(device));
    }

    private class DevicesRepository : IDevicesRepository
    {
        private readonly List<Device> _devices = new List<Device>();

        public Task AddDevice(Device device)
        {
            _devices.Add(device);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Device>> GetDevicesAsync()
        {
            return Task.FromResult<IEnumerable<Device>>(_devices);
        }
    }
}