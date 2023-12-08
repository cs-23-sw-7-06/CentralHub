using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class GetLatestOccupancyResponse
{
    [JsonConstructor]
    public GetLatestOccupancyResponse(bool success, int? occupancy)
    {
        Success = success;
        EstimatedOccupancy = occupancy;
    }

    public bool Success { get; }

    public int? EstimatedOccupancy { get; }

    public static GetLatestOccupancyResponse CreateUnsuccessful()
    {
        return new GetLatestOccupancyResponse(false, null);
    }

    public static GetLatestOccupancyResponse CreateSuccessful(int occupancy)
    {
        return new GetLatestOccupancyResponse(true, occupancy);
    }
}