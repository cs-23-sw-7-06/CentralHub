using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

public sealed class RemoveTrackerResponse
{
    public bool Success { get; }

    [JsonConstructor]
    public RemoveTrackerResponse(bool success)
    {
        Success = success;
    }

    public static RemoveTrackerResponse CreateUnsuccessful()
    {
        return new RemoveTrackerResponse(false);
    }

    public static RemoveTrackerResponse CreateSuccessful()
    {
        return new RemoveTrackerResponse(true);
    }
}