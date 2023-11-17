using System.Text.Json.Serialization;

namespace CentralHub.Api.Dtos;

public sealed class RoomDto
{

    public int RoomDtoId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    /// <summary>
    /// Gets a collection of all trackers in the room.
    /// </summary>
    public ICollection<TrackerDto> Trackers { get; set; } = new List<TrackerDto>();
}