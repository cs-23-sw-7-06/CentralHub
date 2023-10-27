using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDevicesRepository _devicesRepository;
    public DevicesController(IDevicesRepository devicesRepository)
    {
        _devicesRepository = devicesRepository;
    }

    [HttpGet(Name = "GetDevices")]
    public async Task<IEnumerable<Device>> Get()
    {
        return await _devicesRepository.GetDevicesAsync();
    }

    [HttpPost(Name = "PostDevice")]
    public async Task Post(Device device)
    {
        await _devicesRepository.AddDevice(device);
    }
}