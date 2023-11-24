using System.Collections.Immutable;
using CentralHub.Api.Model.Responses.Room;
using CentralHub.Api.Model.Responses.Tracker;

namespace CentralHub.WebUI.Data;

public sealed class TrackerService
{
    private readonly IHttpClientFactory _clientFactory;

    public TrackerService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<Tracker[]> GetTrackersAsync(Room room, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"http://localhost:8081/tracker/all?roomId={room.RoomId}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var trackers = await response.Content.ReadFromJsonAsync<GetTrackersResponse>(cancellationToken: cancellationToken);

        if (trackers == null || !trackers.Success)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return trackers.Trackers.ToArray();
    }

    public async Task<UnregisteredTracker[]> GetUnregisteredTrackersAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"http://localhost:8081/tracker/registration/unregistered");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var unregisteredTrackers = await response.Content.ReadFromJsonAsync<GetUnregisteredTrackersResponse>(cancellationToken: cancellationToken);

        if (unregisteredTrackers == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return unregisteredTrackers.UnregisteredTrackers.ToArray();
    }
}