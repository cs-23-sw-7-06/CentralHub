using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Room;

public sealed class UpdateRoomRequest
{
    [JsonConstructor]
    public UpdateRoomRequest(int roomId, string name, string description)
    {
        RoomId = roomId;
        Name = name;
        Description = description;
    }

    public int RoomId { get; }

    public string Name { get; }

    public string Description { get; }
}