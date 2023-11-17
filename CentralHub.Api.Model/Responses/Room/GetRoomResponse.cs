using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

public sealed class GetRoomResponse
{
    [JsonConstructor]
    public GetRoomResponse(bool success, Room? room)
    {
        Success = success;
        Room = room;
    }

    public bool Success { get; }

    // Null when unsuccessful
    public Room? Room { get; }

    public static GetRoomResponse CreateUnsuccessful()
    {
        return new GetRoomResponse(false, null);
    }

    public static GetRoomResponse CreateSuccessful(Room room)
    {
        return new GetRoomResponse(true, room);
    }
}