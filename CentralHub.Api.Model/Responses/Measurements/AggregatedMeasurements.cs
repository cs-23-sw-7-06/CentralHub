using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurements;

[method: JsonConstructor]
public sealed class AggregatedMeasurements(int aggregatedMeasurementsId,
    DateTime startTime,
    DateTime endTime,
    int bluetoothCount,
    int wifiCount)
{
    public int AggregatedMeasurementsId { get; } = aggregatedMeasurementsId;

    public DateTime StartTime { get; } = startTime;

    public DateTime EndTime { get; } = endTime;

    public int BluetoothCount { get; } = bluetoothCount;

    public int WifiCount { get; } = wifiCount;
}