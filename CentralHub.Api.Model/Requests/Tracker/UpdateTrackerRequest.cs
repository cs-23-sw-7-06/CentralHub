using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests;

public sealed class UpdateTrackerRequest
{
    [JsonConstructor]
    public UpdateTrackerRequest(int trackerId, string name, string description)
    {
        TrackerId = trackerId;
        Name = name;
        Description = description;
    }

    public int TrackerId { get; }

    public string Name { get; }

    public string Description { get; }

    // TODO: Should you be able to change the mac-address?
}