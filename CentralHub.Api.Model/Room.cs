using System.Text.Json.Serialization;

namespace CentralHub.Api.Model;

public sealed class Room
{
    [Obsolete("For deserialization only")]
    [JsonConstructor]
    public Room(int roomId, string name, string description)
    {
        RoomId = roomId;
        Name = name;
        Description = description;
        Trackers = new List<Tracker>();
    }

    public Room(string name, string description)
    {
        RoomId = -1;
        Name = name;
        Description = description;
        Trackers = new List<Tracker>();
    }

    [Obsolete("Only for use with EF Core")]
    public Room(string name, string description, ICollection<Tracker> trackers)
    {
        Name = name;
        Description = description;
        Trackers = trackers;
    }

    public int RoomId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Gets a collection of all trackers in the room.
    /// </summary>
    [JsonIgnore]
    public ICollection<Tracker> Trackers { get; }
}