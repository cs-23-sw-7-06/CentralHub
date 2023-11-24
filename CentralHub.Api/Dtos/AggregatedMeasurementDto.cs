using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Dtos;

public sealed class AggregatedMeasurementDto
{
    public int AggregatedMeasurementDtoId { get; set; }

    public required DateTime StartTime { get; set; }

    public required DateTime EndTime { get; set; }

    public required int MeasurementGroupCount { get; set; }

    public required int BluetoothMedian { get; set; }

    public required double BluetoothMean { get; set; }

    public required int BluetoothMin { get; set; }

    public required int BluetoothMax { get; set; }

    public required int TotalBluetoothDeviceCount { get; set; }

    public required int WifiMedian { get; set; }

    public required double WifiMean { get; set; }

    public required int WifiMin { get; set; }

    public required int WifiMax { get; set; }

    public required int TotalWifiDeviceCount { get; set; }

    public required int RoomDtoId { get; set; }

    public required RoomDto RoomDto { get; set; }
}