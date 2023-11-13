using System.ComponentModel;

namespace CentralHub.Api.Model;

public sealed class Measurement
{
    private Protocol _protocol;

    [Obsolete("Deserialization only")]
    public Measurement()
    {
    }

    public Measurement(Protocol type, string macAddress, int rssi)
    {
        Type = type;
        MacAddress = macAddress;
        Rssi = rssi;
    }

    public string MacAddress { get; set; }

    public enum Protocol
    {
        wifi,
        bluetooth
    }
    public Protocol Type
    {
        get => _protocol;
        set
        {
            if (value == Protocol.wifi || value == Protocol.bluetooth) _protocol = value;
            else throw new InvalidEnumArgumentException("Invalid Protocol Type");
        }
    }

    public int Rssi { get; set; }
}