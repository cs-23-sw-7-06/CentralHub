using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Room;

public sealed class RemoveRoomResponse
{
    public bool Success { get; }

    [JsonConstructor]
    public RemoveRoomResponse(bool success)
    {
        Success = success;
    }

    public static RemoveRoomResponse CreateUnsuccessful()
    {
        return new RemoveRoomResponse(false);
    }


    public static RemoveRoomResponse CreateSuccessful()
    {
        return new RemoveRoomResponse(true);
    }
}