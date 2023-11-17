using System.Text.Json.Serialization;

namespace CentralHub.Api.Dtos;

public sealed class TrackerDto
{
    public int TrackerDtoId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string MacAddress { get; set; }

    public required int RoomDtoId { get; set; }

    public required RoomDto RoomDto { get; set; }
}
