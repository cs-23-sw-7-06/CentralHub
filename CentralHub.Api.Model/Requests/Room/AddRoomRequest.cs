using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Room;

[method: JsonConstructor]
public sealed class AddRoomRequest(string name, string description, int capacity, List<int> neighbourIds)
{
    public string Name { get; } = name;

    public string Description { get; } = description;

    public int Capacity { get; } = capacity;

    public List<int> NeighbourIds { get; } = neighbourIds;
}