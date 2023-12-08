using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class GetUnregisteredTrackersResponse(IEnumerable<UnregisteredTracker> unregisteredTrackers)
{
    public IEnumerable<UnregisteredTracker> UnregisteredTrackers { get; } = unregisteredTrackers;
}