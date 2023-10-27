using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface IDevicesRepository
{
    Task AddDevice(Device device);
    Task<IEnumerable<Device>> GetDevicesAsync();
}