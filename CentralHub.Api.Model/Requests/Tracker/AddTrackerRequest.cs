using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Tracker;

[method: JsonConstructor]
public sealed class AddTrackerRequest(
    int roomId,
    string name,
    string description,
    string wifiMacAddress,
    string bluetoothMacAddress)
{
    public int RoomId { get; } = roomId;

    public string Name { get; } = name;

    public string Description { get; } = description;

    public string WifiMacAddress { get; } = wifiMacAddress;

    public string BluetoothMacAddress { get; } = bluetoothMacAddress;
}