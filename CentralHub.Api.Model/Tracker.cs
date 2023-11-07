using System.Text.Json.Serialization;

namespace CentralHub.Api.Model;

public sealed class Tracker
{
    [Obsolete("Deserialization only")]
    public Tracker()
    {
    }

    public Tracker(string name, string description, string macAddress, Room room)
    {
        Name = name;
        Description = description;
        MacAddress = macAddress;
        Room = room;
        RoomId = room.RoomId;
    }

    [Obsolete("EF Core only!")]
    public Tracker(string name, string description, string macAddress)
        : this(name, description, macAddress, null)
    {
    }

    public int TrackerId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string MacAddress { get; set; }

    public int RoomId { get; set; }

    // We should not serialize / deserialize the room,
    // as the room should be a reference to an already existing room.
    [JsonIgnore]
    public Room Room { get; set; }
}
