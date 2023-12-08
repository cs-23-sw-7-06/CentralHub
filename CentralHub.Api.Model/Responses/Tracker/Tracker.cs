using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class Tracker(
    int trackerId,
    string name,
    string description,
    string wifiMacAddress,
    string bluetoothMacAddress,
    int roomId)
{
    public int TrackerId { get; } = trackerId;

    public string Name { get; } = name;

    public string Description { get; } = description;

    public string WifiMacAddress { get; } = wifiMacAddress;

    public string BluetoothMacAddress { get; } = bluetoothMacAddress;

    public int RoomId { get; } = roomId;
}