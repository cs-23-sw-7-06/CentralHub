using System.Text.Json.Serialization;
using CentralHub.Api.Model.Requests.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class AddTrackerResponse(bool success, int? trackerId)
{
    public bool Success { get; } = success;

    // Null when unsuccessful
    public int? TrackerId { get; } = trackerId;

    public static AddTrackerResponse CreateUnsuccessful()
    {
        return new AddTrackerResponse(false, null);
    }

    public static AddTrackerResponse CreateSuccessful(int trackerId)
    {
        return new AddTrackerResponse(true, trackerId);
    }
}