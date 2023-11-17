using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class Tracker
{
    [JsonConstructor]
    public Tracker(int trackerId, string name, string description, string macAddress, int roomId)
    {
        TrackerId = trackerId;
        Name = name;
        Description = description;
        MacAddress = macAddress;
        RoomId = roomId;
    }

    public int TrackerId { get; }

    public string Name { get; }

    public string Description { get; }

    public string MacAddress { get; }

    public int RoomId { get; }
}