using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

[method: JsonConstructor]
public sealed class Room(int roomId, string name, string description)
{
    public int RoomId { get; } = roomId;

    public string Name { get; } = name;

    public string Description { get; } = description;
}
