using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests.Room;
using CentralHub.Api.Model.Responses.Room;
using CentralHub.Api.Model.Responses.Tracker;
using Microsoft.AspNetCore.Mvc.Formatters;

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
            $"http://localhost:8081/room/remove?roomId={room.RoomId}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var removeRoomResponse = await response.Content.ReadFromJsonAsync<RemoveRoomResponse>(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !removeRoomResponse!.Success)
        {
            throw new InvalidOperationException("Somethings fucky");
        }
    }

    public async Task UpdateRoomAsync(Room room, string name, string description, CancellationToken cancellationToken)
    {
        var updateRoomRequest = new UpdateRoomRequest(room.RoomId, name, description);
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"http://localhost:8081/room/update"
            );
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");
        request.Content = new StringContent(JsonSerializer.Serialize(updateRoomRequest), Encoding.UTF8, "application/json");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);
        var updateRoomResponse = await response.Content.ReadFromJsonAsync<UpdateRoomResponse>(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !updateRoomResponse!.Success)
        {
            throw new InvalidOperationException("Somethings fucky");
        }
    }

    public async Task<int> AddRoomAsync(string name, string description, CancellationToken cancellationToken)
    {
        var addRoomRequest = new AddRoomRequest(name, description);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"http://localhost:8081/room/add"
            );
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");
        request.Content = new StringContent(JsonSerializer.Serialize(addRoomRequest), Encoding.UTF8, "application/json");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Somethings fucky");
        }

        var addRoomResponse = await response.Content.ReadFromJsonAsync<AddRoomResponse>(cancellationToken);
        return addRoomResponse!.RoomId;
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

}
