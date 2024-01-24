using System.Text.Json.Serialization;

namespace CentralHub.Api.Dtos;

public sealed class RoomDto
{

    public int RoomDtoId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required int Capacity { get; set; }

    public ICollection<RoomDto> NeighbouringRooms { get; set; } = new List<RoomDto>();

    /// <summary>
    /// Gets a collection of all trackers in the room.
    /// </summary>
    public ICollection<TrackerDto> Trackers { get; set; } = new List<TrackerDto>();

    public ICollection<AggregatedMeasurementDto> AggregatedMeasurements { get; set; } = new List<AggregatedMeasurementDto>();
}