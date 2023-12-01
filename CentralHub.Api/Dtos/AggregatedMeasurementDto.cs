namespace CentralHub.Api.Dtos;

public sealed class AggregatedMeasurementDto
{
    public int AggregatedMeasurementDtoId { get; set; }

    public required DateTime StartTime { get; set; }

    public required DateTime EndTime { get; set; }

    public required int MeasurementGroupCount { get; set; }

    public required int BluetoothMedianDeviceCount { get; set; }

    public required double BluetoothMeanDeviceCount { get; set; }

    public required int BluetoothMinDeviceCount { get; set; }

    public required int BluetoothMaxDeviceCount { get; set; }

    public required int TotalBluetoothDeviceCount { get; set; }

    public required int WifiMedianDeviceCount { get; set; }

    public required double WifiMeanDeviceCount { get; set; }

    public required int WifiMinDeviceCount { get; set; }

    public required int WifiMaxDeviceCount { get; set; }

    public required int TotalWifiDeviceCount { get; set; }

    public required int RoomDtoId { get; set; }

    public required RoomDto RoomDto { get; set; }
}