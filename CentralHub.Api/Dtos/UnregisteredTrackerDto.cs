namespace CentralHub.Api.Dtos;

public class UnregisteredTrackerDto
{
    public required string WifiMacAddress { get; set; }

    public required string BluetoothMacAddress { get; set; }
}