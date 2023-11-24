using System.Text.Json.Serialization;

namespace CentralHub.Api.Model;

public sealed class Measurement
{
    [JsonConstructor]
    public Measurement(string macAddress, Protocol type, int rssi)
    {
        MacAddress = macAddress;
        Type = type;
        Rssi = rssi;
    }

    public string MacAddress { get; }

    public Protocol Type { get; }

    public int Rssi { get; }

    public enum Protocol
    {
        Wifi,
        Bluetooth
    }
}