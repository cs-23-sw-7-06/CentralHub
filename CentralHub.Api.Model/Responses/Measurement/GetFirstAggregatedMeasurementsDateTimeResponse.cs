using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class GetFirstAggregatedMeasurementsDateTimeResponse(bool success, DateTime? firstDateTime)
{
    public bool Success { get; } = success;
    public DateTime? FirstDateTime { get; } = firstDateTime;

    public static GetFirstAggregatedMeasurementsDateTimeResponse CreateUnsuccessful()
    {
        return new GetFirstAggregatedMeasurementsDateTimeResponse(false, null);
    }

    public static GetFirstAggregatedMeasurementsDateTimeResponse CreateSuccessful(DateTime firstDateTime)
    {
        return new GetFirstAggregatedMeasurementsDateTimeResponse(true, firstDateTime);
    }
}