using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

[method: JsonConstructor]
public sealed class AddRoomResponse(int roomId)
{
    public int RoomId { get; } = roomId;
}