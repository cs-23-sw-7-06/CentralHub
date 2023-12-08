using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Room;

[method: JsonConstructor]
public sealed class AddRoomRequest(string name, string description)
{
    public string Name { get; } = name;

    public string Description { get; } = description;
}