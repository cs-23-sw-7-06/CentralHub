using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Tracker;

[method: JsonConstructor]
public sealed class RemoveTrackerResponse(bool success)
{
    public bool Success { get; } = success;

    public static RemoveTrackerResponse CreateUnsuccessful()
    {
        return new RemoveTrackerResponse(false);
    }

    public static RemoveTrackerResponse CreateSuccessful()
    {
        return new RemoveTrackerResponse(true);
    }
}