using System.Text.Json.Serialization;
using CentralHub.Api.Model.Responses.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class GetTrackersResponse(bool success, IEnumerable<Tracker>? trackers)
{
    public bool Success { get; } = success;

    // Null when unsuccessful
    public IEnumerable<Tracker>? Trackers { get; } = trackers;

    public static GetTrackersResponse CreateUnsuccessful()
    {
        return new GetTrackersResponse(false, null);
    }

    public static GetTrackersResponse CreateSuccessful(IEnumerable<Tracker> trackers)
    {
        return new GetTrackersResponse(true, trackers);
    }
}