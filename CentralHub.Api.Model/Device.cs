namespace CentralHub.Api.Model;

public class Device
{
    public Device(string name, string macAddress, DeviceType deviceType)
    {
        Name = name;
        MacAddress = macAddress;
        DeviceType = deviceType;
    }

    public int DeviceId { get; set; }

    public string Name { get; set; }

    public string MacAddress { get; set; }

    public DeviceType DeviceType { get; set; }
}