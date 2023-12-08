using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class GetLatestOccupancyResponse(bool success, int? occupancy)
{
    public bool Success { get; } = success;

    public int? EstimatedOccupancy { get; } = occupancy;

    public static GetLatestOccupancyResponse CreateUnsuccessful()
    {
        return new GetLatestOccupancyResponse(false, null);
    }

    public static GetLatestOccupancyResponse CreateSuccessful(int occupancy)
    {
        return new GetLatestOccupancyResponse(true, occupancy);
    }
}