using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests;

public sealed class AddTrackerRequest
{
    [JsonConstructor]
    public AddTrackerRequest(int roomId, string name, string description, string macAddress)
    {
        RoomId = roomId;
        Name = name;
        Description = description;
        MacAddress = macAddress;
    }

    public int RoomId { get; }

    public string Name { get; }

    public string Description { get; }

    public string MacAddress { get; }
}