using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

[method: JsonConstructor]
public sealed class GetRoomResponse(bool success, Room? room)
{
    public bool Success { get; } = success;

    // Null when unsuccessful
    public Room? Room { get; } = room;

    public static GetRoomResponse CreateUnsuccessful()
    {
        return new GetRoomResponse(false, null);
    }

    public static GetRoomResponse CreateSuccessful(Room room)
    {
        return new GetRoomResponse(true, room);
    }
}