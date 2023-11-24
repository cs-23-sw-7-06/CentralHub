using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class GetUnregisteredTrackersResponse
{
    public IEnumerable<UnregisteredTracker> UnregisteredTrackers { get; }

    [JsonConstructor]
    public GetUnregisteredTrackersResponse(IEnumerable<UnregisteredTracker> unregisteredTrackers)
    {
        UnregisteredTrackers = unregisteredTrackers;
    }
}