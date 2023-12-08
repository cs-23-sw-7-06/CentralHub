namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class UnregisteredTracker(string wifiMacAddress, string bluetoothMacAddress)
{
    public string WifiMacAddress { get; } = wifiMacAddress;
    public string BluetoothMacAddress { get; } = bluetoothMacAddress;
}