using System.Text.Json.Serialization;
using CentralHub.Api.Model.Requests.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class UpdateTrackerResponse
{
    public bool Success { get; }

    [JsonConstructor]
    public UpdateTrackerResponse(bool success)
    {
        Success = success;
    }

    public static UpdateTrackerResponse CreateUnsuccessful()
    {
        return new UpdateTrackerResponse(false);
    }

    public static UpdateTrackerResponse CreateSuccessful()
    {
        return new UpdateTrackerResponse(true);
    }
}