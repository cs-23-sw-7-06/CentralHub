using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

public sealed class AddRoomResponse
{
    [JsonConstructor]
    public AddRoomResponse(int roomId)
    {
        RoomId = roomId;
    }

    public int RoomId { get; }
}