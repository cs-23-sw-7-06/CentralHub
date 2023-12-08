using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Responses.AggregatedMeasurements;

public sealed class SetDevicesPerPersonResponse
{
    [JsonConstructor]
    public SetDevicesPerPersonResponse(bool success, float? bluetoothDevicesPerPerson, float? wifiDevicesPerPerson)
    {
        Success = success;
        BluetoothDevicesPerPerson = bluetoothDevicesPerPerson;
        WifiDevicesPerPerson = wifiDevicesPerPerson;
    }

    public bool Success { get; }
    public float? BluetoothDevicesPerPerson { get; }
    public float? WifiDevicesPerPerson { get; }

    public static SetDevicesPerPersonResponse CreateUnsuccessful()
    {
        return new SetDevicesPerPersonResponse(false, null, null);
    }

    public static SetDevicesPerPersonResponse CreateSuccessful(float bluetoothDevicesPerPerson, float wifiDevicesPerPerson)
    {
        return new SetDevicesPerPersonResponse(true, bluetoothDevicesPerPerson, wifiDevicesPerPerson);
    }
}