using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Services;

public class DevicesRepository : IDevicesRepository
{
    private readonly DevicesContext _devicesContext;

    public DevicesRepository(DevicesContext devicesContext)
    {
        _devicesContext = devicesContext;
        _devicesContext.Database.OpenConnection();
        _devicesContext.Database.EnsureCreated();
    }

    public async Task AddDevice(Device device)
    {
        await _devicesContext.Devices.AddAsync(device);
        await _devicesContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Device>> GetDevicesAsync()
    {
        return await _devicesContext.Devices.ToArrayAsync();
    }
}