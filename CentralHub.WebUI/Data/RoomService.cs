using System.Net;
using CentralHub.Api.Model;

namespace CentralHub.WebUI.Data;

public sealed class RoomService
{
    private readonly IHttpClientFactory _clientFactory;

    public RoomService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task RemoveRoomAsync(Room room, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"http://localhost:8081/room/remove?id={room.RoomId}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Somethings fucky");
        }
    }

    public async Task<Room[]> GetRoomsAsync(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "http://localhost:8081/room/all");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var rooms = await response.Content.ReadFromJsonAsync<Room[]>(cancellationToken: cancellationToken);

        if (rooms == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return rooms;
    }

    public async Task<Tracker[]> GetTrackersAsync(Room room, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"http://localhost:8081/tracker/all?id={room.RoomId}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var trackers = await response.Content.ReadFromJsonAsync<Tracker[]>(cancellationToken: cancellationToken);

        if (trackers == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return trackers;
    }
}
