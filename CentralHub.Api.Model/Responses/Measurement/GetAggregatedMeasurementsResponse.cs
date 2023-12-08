using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class GetAggregatedMeasurementsResponse(
    bool success,
    IReadOnlyCollection<AggregatedMeasurements>? aggregatedMeasurements)
{
    public bool Success { get; } = success;

    // Null when unsuccessful
    public IReadOnlyCollection<AggregatedMeasurements>? AggregatedMeasurements { get; } = aggregatedMeasurements;

    public static GetAggregatedMeasurementsResponse CreateUnsuccessful()
    {
        return new GetAggregatedMeasurementsResponse(false, null);
    }

    public static GetAggregatedMeasurementsResponse CreateSuccessful(IReadOnlyCollection<AggregatedMeasurements> aggregatedMeasurements)
    {
        return new GetAggregatedMeasurementsResponse(true, aggregatedMeasurements);
    }
}