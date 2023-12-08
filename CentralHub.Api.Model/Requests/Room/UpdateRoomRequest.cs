using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Room;

[method: JsonConstructor]
public sealed class UpdateRoomRequest(int roomId, string name, string description)
{
    public int RoomId { get; } = roomId;

    public string Name { get; } = name;

    public string Description { get; } = description;
}