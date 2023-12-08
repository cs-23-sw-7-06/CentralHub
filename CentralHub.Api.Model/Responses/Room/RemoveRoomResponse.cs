using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

[method: JsonConstructor]
public sealed class RemoveRoomResponse(bool success)
{
    public bool Success { get; } = success;

    public static RemoveRoomResponse CreateUnsuccessful()
    {
        return new RemoveRoomResponse(false);
    }


    public static RemoveRoomResponse CreateSuccessful()
    {
        return new RemoveRoomResponse(true);
    }
}