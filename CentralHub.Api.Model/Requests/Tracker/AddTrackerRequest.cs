using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Tracker;

public sealed class AddTrackerRequest
{
    [JsonConstructor]
    public AddTrackerRequest(int roomId, string name, string description, string wifiMacAddress, string bluetoothMacAddress)
    {
        RoomId = roomId;
        Name = name;
        Description = description;
        WifiMacAddress = wifiMacAddress;
        BluetoothMacAddress = bluetoothMacAddress;
    }

    public int RoomId { get; }

    public string Name { get; }

    public string Description { get; }

    public string WifiMacAddress { get; }

    public string BluetoothMacAddress { get; }
}