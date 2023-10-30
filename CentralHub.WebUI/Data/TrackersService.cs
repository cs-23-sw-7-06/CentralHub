using CentralHub.Api.Model;

namespace CentralHub.WebUI.Data;

public class TrackersService
{
    private readonly IHttpClientFactory _clientFactory;

    public TrackersService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<Tracker[]> GetTrackersAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://localhost:8081/Trackers");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request);
        var trackers = await response.Content.ReadFromJsonAsync<Tracker[]>();

        if (trackers == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return trackers;
    }
}
