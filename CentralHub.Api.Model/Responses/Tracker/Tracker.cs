using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class Tracker
{
    [JsonConstructor]
    public Tracker(int trackerId, string name, string description, string wifiMacAddress, string bluetoothMacAddress, int roomId)
    {
        TrackerId = trackerId;
        Name = name;
        Description = description;
        WifiMacAddress = wifiMacAddress;
        BluetoothMacAddress = bluetoothMacAddress;
        RoomId = roomId;
    }

    public int TrackerId { get; }

    public string Name { get; }

    public string Description { get; }

    public string WifiMacAddress { get; }

    public string BluetoothMacAddress { get; }

    public int RoomId { get; }
}