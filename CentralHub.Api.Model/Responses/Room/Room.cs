using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

public sealed class Room
{
    [JsonConstructor]
    public Room(int roomId, string name, string description)
    {
        RoomId = roomId;
        Name = name;
        Description = description;
    }

    public int RoomId { get; }

    public string Name { get; }

    public string Description { get; }
}
