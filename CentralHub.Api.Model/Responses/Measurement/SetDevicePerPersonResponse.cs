using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class SetSettingsResponse(bool success)
{
    public bool Success { get; } = success;

    public static SetSettingsResponse CreateUnsuccessful()
    {
        return new SetSettingsResponse(false);
    }

    public static SetSettingsResponse CreateSuccessful()
    {
        return new SetSettingsResponse(true);
    }
}