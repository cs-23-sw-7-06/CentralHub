using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class AddMeasurementsResponse
{
    [JsonConstructor]
    public AddMeasurementsResponse(bool success)
    {
        Success = success;
    }

    public bool Success { get; }

    public static AddMeasurementsResponse CreateUnsuccessful()
    {
        return new AddMeasurementsResponse(false);
    }

    public static AddMeasurementsResponse CreateSuccessful()
    {
        return new AddMeasurementsResponse(true);
    }
}