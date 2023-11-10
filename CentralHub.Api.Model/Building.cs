using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace CentralHub.Api.Model;

public sealed class Building
{
    [Obsolete("Only for Deserialization")]
    public Building()
    {
        Rooms = new List<Room>();
    }

    public Building(string name, string description)
#pragma warning disable CS0618 // Type or member is obsolete
        : this(name, description, new List<Room>())
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }

    [Obsolete("Only for use with EF Core")]
    public Building(string name, string description, ICollection<Room> rooms)
    {
        Name = name;
        Description = description;
        Rooms = rooms;
    }

    public int BuildingId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Gets a collection of all rooms in the room.
    /// </summary>
    [JsonIgnore]
    public ICollection<Room> Rooms { get; }
}
