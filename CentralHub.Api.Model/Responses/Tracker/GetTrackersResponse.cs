using System.Text.Json.Serialization;
using CentralHub.Api.Model.Responses.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class GetTrackersResponse
{
    [JsonConstructor]
    public GetTrackersResponse(bool success, IEnumerable<Tracker>? trackers)
    {
        Success = success;
        Trackers = trackers;
    }

    public bool Success { get; }

    // Null when unsuccessful
    public IEnumerable<Tracker>? Trackers { get; }

    public static GetTrackersResponse CreateUnsuccessful()
    {
        return new GetTrackersResponse(false, null);
    }

    public static GetTrackersResponse CreateSuccessful(IEnumerable<Tracker> trackers)
    {
        return new GetTrackersResponse(true, trackers);
    }
}