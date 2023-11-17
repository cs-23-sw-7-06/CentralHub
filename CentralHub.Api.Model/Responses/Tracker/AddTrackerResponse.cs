using System.Text.Json.Serialization;
using CentralHub.Api.Model.Requests.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class AddTrackerResponse
{
    [JsonConstructor]
    public AddTrackerResponse(bool success, int? trackerId)
    {
        Success = success;
        TrackerId = trackerId;
    }

    public bool Success { get; }

    // Null when unsuccessful
    public int? TrackerId { get; }

    public static AddTrackerResponse CreateUnsuccessful()
    {
        return new AddTrackerResponse(false, null);
    }

    public static AddTrackerResponse CreateSuccessful(int trackerId)
    {
        return new AddTrackerResponse(true, trackerId);
    }
}