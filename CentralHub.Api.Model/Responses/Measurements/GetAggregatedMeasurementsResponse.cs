using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class GetAggregatedMeasurementsResponse
{
    [JsonConstructor]
    public GetAggregatedMeasurementsResponse(bool success, IReadOnlyCollection<AggregatedMeasurements>? aggregatedMeasurements)
    {
        Success = success;
        AggregatedMeasurements = aggregatedMeasurements;
    }

    public bool Success { get; }

    // Null when unsuccessful
    public IReadOnlyCollection<AggregatedMeasurements>? AggregatedMeasurements { get; }

    public static GetAggregatedMeasurementsResponse CreateUnsuccessful()
    {
        return new GetAggregatedMeasurementsResponse(false, null);
    }

    public static GetAggregatedMeasurementsResponse CreateSuccessful(IReadOnlyCollection<AggregatedMeasurements> aggregatedMeasurements)
    {
        return new GetAggregatedMeasurementsResponse(true, aggregatedMeasurements);
    }
}