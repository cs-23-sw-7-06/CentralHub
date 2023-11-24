using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class AggregatedMeasurements
{
    [JsonConstructor]
    public AggregatedMeasurements(
        int aggregatedMeasurementDtoId,
        DateTime startTime,
        DateTime endTime,
        int measurementGroupCount,
        int bluetoothMedian,
        double bluetoothMean,
        int bluetoothMin,
        int bluetoothMax,
        int totalBluetoothDeviceCount,
        int wifiMedian,
        double wifiMean,
        int wifiMin,
        int wifiMax,
        int totalWifiDeviceCount)
    {
        AggregatedMeasurementDtoId = aggregatedMeasurementDtoId;
        StartTime = startTime;
        EndTime = endTime;
        MeasurementGroupCount = measurementGroupCount;
        BluetoothMedian = bluetoothMedian;
        BluetoothMean = bluetoothMean;
        BluetoothMin = bluetoothMin;
        BluetoothMax = bluetoothMax;
        TotalBluetoothDeviceCount = totalBluetoothDeviceCount;
        WifiMedian = wifiMedian;
        WifiMean = wifiMean;
        WifiMin = wifiMin;
        WifiMax = wifiMax;
        TotalWifiDeviceCount = totalWifiDeviceCount;
    }

    public int AggregatedMeasurementDtoId { get; }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public int MeasurementGroupCount { get; }

    public int BluetoothMedian { get; }

    public double BluetoothMean { get; }

    public int BluetoothMin { get; }

    public int BluetoothMax { get; }

    public int TotalBluetoothDeviceCount { get; }

    public int WifiMedian { get; }

    public double WifiMean { get; }

    public int WifiMin { get; }

    public int WifiMax { get; }

    public int TotalWifiDeviceCount { get; }
}