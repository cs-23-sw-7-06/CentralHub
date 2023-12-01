using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class AddMeasurementResponse
{
    [JsonConstructor]
    public AddMeasurementResponse(bool success)
    {
        Success = success;
    }

    public bool Success { get; }

    public static AddMeasurementResponse CreateUnsuccessful()
    {
        return new AddMeasurementResponse(false);
    }

    public static AddMeasurementResponse CreateSuccessful()
    {
        return new AddMeasurementResponse(true);
    }
}