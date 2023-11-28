using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

public sealed class UpdateRoomResponse
{
    public bool Success { get; }

    [JsonConstructor]
    public UpdateRoomResponse(bool success)
    {
        Success = success;
    }

    public static UpdateRoomResponse CreateUnsuccessful()
    {
        return new UpdateRoomResponse(false);
    }

    public static UpdateRoomResponse CreateSuccessful()
    {
        return new UpdateRoomResponse(true);
    }
}