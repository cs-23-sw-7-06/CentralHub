using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class GetLatestOccupancyResponse
{
    [JsonConstructor]
    public GetLatestOccupancyResponse(int? occupancy, bool success)
    {
        EstimatedOccupancy = occupancy;
        Success = success;
    }

    public bool Success { get; }

    public int? EstimatedOccupancy { get; }

    public static GetLatestOccupancyResponse CreateUnsuccessful()
    {
        return new GetLatestOccupancyResponse(null, false);
    }

    public static GetLatestOccupancyResponse CreateSuccessful(int occupancy)
    {
        return new GetLatestOccupancyResponse(occupancy, true);
    }
}