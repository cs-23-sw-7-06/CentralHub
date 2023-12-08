using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

[method: JsonConstructor]
public sealed class UpdateRoomResponse(bool success)
{
    public bool Success { get; } = success;

    public static UpdateRoomResponse CreateUnsuccessful()
    {
        return new UpdateRoomResponse(false);
    }

    public static UpdateRoomResponse CreateSuccessful()
    {
        return new UpdateRoomResponse(true);
    }
}