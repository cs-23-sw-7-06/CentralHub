using CentralHub.Api.Model;

namespace CentralHub.WebUI.Data;

public class DevicesService
{
    private readonly IHttpClientFactory _clientFactory;

    public DevicesService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<Device[]> GetDevicesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://localhost:5003/Devices");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request);
        var devices = await response.Content.ReadFromJsonAsync<Device[]>();

        if (devices == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return devices;
    }
}