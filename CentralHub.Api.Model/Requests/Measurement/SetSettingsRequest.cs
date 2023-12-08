using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Measurement;

[method: JsonConstructor]
public sealed class SetSettingsRequest(float bluetoothDevicesPerPerson, float wifiDevicesPerPerson)
{
    public float BluetoothDevicesPerPerson { get; } = bluetoothDevicesPerPerson;
    public float WifiDevicesPerPerson { get; } = wifiDevicesPerPerson;
}