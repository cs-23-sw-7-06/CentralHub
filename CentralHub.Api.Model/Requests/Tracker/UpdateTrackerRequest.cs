using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Tracker;

[method: JsonConstructor]
public sealed class UpdateTrackerRequest(int trackerId, string name, string description)
{
    public int TrackerId { get; } = trackerId;

    public string Name { get; } = name;

    public string Description { get; } = description;

    // TODO: Should you be able to change the mac-address?
}