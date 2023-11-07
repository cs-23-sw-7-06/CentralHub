using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CentralHub.Api.Model;

public sealed class Room
{
    [Obsolete("Only for Deserialization")]
    public Room()
    {
    }

    public Room(string name, string description)
#pragma warning disable CS0618 // Type or member is obsolete
        : this(name, description, new List<Tracker>())
#pragma warning restore CS0618 // Type or member is obsolete
    {
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