namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class UnregisteredTracker
{
    public string WifiMacAddress { get; }
    public string BluetoothMacAddress { get; }

    public UnregisteredTracker(string wifiMacAddress, string bluetoothMacAddress)
    {
        WifiMacAddress = wifiMacAddress;
        BluetoothMacAddress = bluetoothMacAddress;
    }
}