using System.Text.Json.Serialization;
using CentralHub.Api.Model.Requests.Room;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class UpdateTrackerResponse(bool success)
{
    public bool Success { get; } = success;

    public static UpdateTrackerResponse CreateUnsuccessful()
    {
        return new UpdateTrackerResponse(false);
    }

    public static UpdateTrackerResponse CreateSuccessful()
    {
        return new UpdateTrackerResponse(true);
    }
}