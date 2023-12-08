using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class AddMeasurementsResponse(bool success)
{
    public bool Success { get; } = success;

    public static AddMeasurementsResponse CreateUnsuccessful()
    {
        return new AddMeasurementsResponse(false);
    }

    public static AddMeasurementsResponse CreateSuccessful()
    {
        return new AddMeasurementsResponse(true);
    }
}