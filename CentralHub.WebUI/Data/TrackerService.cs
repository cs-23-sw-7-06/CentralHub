using System.Net;
using System.Text.Json;
using System.Collections.Immutable;
using CentralHub.Api.Model.Responses.Room;
using CentralHub.Api.Model.Responses.Tracker;
using CentralHub.Api.Model.Requests.Tracker;
using System.Net.Http.Headers;
using System.Text;
using CentralHub.Api.Model;
using Microsoft.AspNetCore.Mvc.Formatters;
using CentralHub.Api.Model.Requests;

namespace CentralHub.WebUI.Data;

public sealed class TrackerService
{
    private readonly IHttpClientFactory _clientFactory;

    public TrackerService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task RemoveTrackerAsync(Tracker tracker, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"http://localhost:8081/tracker/remove?trackerId={tracker.TrackerId}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var removeTrackerResponse = await response.Content.ReadFromJsonAsync<RemoveTrackerResponse>(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !removeTrackerResponse!.Success)
        {
            throw new InvalidOperationException("Somethings fucky");
        }
    }

    public async Task AddTrackerAsync(Room room, UnregisteredTracker tracker, string name, string description, CancellationToken cancellationToken)
    {
        var addTrackerRequest = new AddTrackerRequest(room.RoomId, name, description, tracker.WifiMacAddress, tracker.BluetoothMacAddress);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"http://localhost:8081/tracker/add"
            );
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");
        request.Content = new StringContent(JsonSerializer.Serialize(addTrackerRequest), Encoding.UTF8, "application/json");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var addTrackerResponse = await response.Content.ReadFromJsonAsync<AddTrackerResponse>(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !addTrackerResponse!.Success)
        {
            throw new InvalidOperationException("Somethings fucky");
        }
    }

    public async Task UpdateTrackerAsync(Room room, Tracker tracker, string name, string description, CancellationToken cancellationToken)
    {
        if (room.RoomId == tracker.RoomId)
        {
            var addTrackerRequest = new UpdateTrackerRequest(tracker.TrackerId, name, description);
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"http://localhost:8081/tracker/update"
                );
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "CentralHub.WebUI");
            request.Content = new StringContent(JsonSerializer.Serialize(addTrackerRequest), Encoding.UTF8, "application/json");

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Somethings fucky");
            }
        }
        else
        {
            await RemoveTrackerAsync(tracker, cancellationToken);
            var uTrackers = await GetUnregisteredTrackersAsync(cancellationToken);
            var uTracker = uTrackers.Single(t => t.WifiMacAddress == tracker.WifiMacAddress);
            await AddTrackerAsync(room, uTracker, name, description, cancellationToken);
        }
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