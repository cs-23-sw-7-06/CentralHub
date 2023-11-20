using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class GetTrackerRegistrationInfoResponse
{
    public bool Registered { get; }

    public int? TrackerId { get; }

    [JsonConstructor]
    public GetTrackerRegistrationInfoResponse(bool registered, int? trackerId)
    {
        Registered = registered;
        TrackerId = trackerId;
    }

    public static GetTrackerRegistrationInfoResponse CreateRegistered(int trackerId)
    {
        return new GetTrackerRegistrationInfoResponse(true, trackerId);
    }

    public static GetTrackerRegistrationInfoResponse CreateUnregistered()
    {
        return new GetTrackerRegistrationInfoResponse(false, null);
    }
}