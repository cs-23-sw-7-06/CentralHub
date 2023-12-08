using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.Measurement;

[method: JsonConstructor]
public sealed class Measurement(string macAddress, Measurement.Protocol type, int rssi)
{
    public string MacAddress { get; } = macAddress;

    public Protocol Type { get; } = type;

    public int Rssi { get; } = rssi;

    public enum Protocol
    {
        Wifi,
        Bluetooth
    }
}