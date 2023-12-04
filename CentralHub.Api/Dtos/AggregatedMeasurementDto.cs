namespace CentralHub.Api.Dtos;

public sealed class AggregatedMeasurementDto
{
    public int AggregatedMeasurementDtoId { get; set; }

    public required DateTime StartTime { get; set; }

    public required DateTime EndTime { get; set; }

    public required int BluetoothCount { get; set; }

    public required int WifiCount { get; set; }

    public required int RoomDtoId { get; set; }

    public required RoomDto RoomDto { get; set; }
}