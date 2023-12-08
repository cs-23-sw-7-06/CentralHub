using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class GetTrackerRegistrationInfoResponse(bool registered, int? trackerId)
{
    public bool Registered { get; } = registered;

    public int? TrackerId { get; } = trackerId;

    public static GetTrackerRegistrationInfoResponse CreateRegistered(int trackerId)
    {
        return new GetTrackerRegistrationInfoResponse(true, trackerId);
    }

    public static GetTrackerRegistrationInfoResponse CreateUnregistered()
    {
        return new GetTrackerRegistrationInfoResponse(false, null);
    }
}