using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests;
using CentralHub.Api.Model.Requests.Tracker;
using CentralHub.Api.Model.Responses.Tracker;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/tracker")]
public class TrackerController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly ITrackerRepository _trackerRepository;

    public TrackerController(IRoomRepository roomRepository, ITrackerRepository trackerRepository)
    {
        _roomRepository = roomRepository;
        _trackerRepository = trackerRepository;
    }

    [HttpPost("add")]
    public async Task<AddTrackerResponse> AddTracker(AddTrackerRequest addTrackerRequest, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomByIdAsync(addTrackerRequest.RoomId, cancellationToken);
        if (room == null)
        {
            return AddTrackerResponse.CreateUnsuccessful();
        }

        var trackerDto = new TrackerDto()
        {
            Name = addTrackerRequest.Name,
            Description = addTrackerRequest.Description,
            WifiMacAddress = addTrackerRequest.WifiMacAddress,
            BluetoothMacAddress = addTrackerRequest.BluetoothMacAddress,
            RoomDtoId = room.RoomDtoId,
            RoomDto = room
        };

        var trackerId = await _trackerRepository.AddTrackerAsync(trackerDto, cancellationToken);

        return AddTrackerResponse.CreateSuccessful(trackerId);
    }

    [HttpPut("update")]
    public async Task<UpdateTrackerResponse> UpdateTracker(UpdateTrackerRequest updateTrackerRequest, CancellationToken cancellationToken)
    {
        var trackerDto = await _trackerRepository.GetTrackerByIdAsync(updateTrackerRequest.TrackerId, cancellationToken);

        if (trackerDto == null)
        {
            return UpdateTrackerResponse.CreateUnsuccessful();
        }

        trackerDto.Name = updateTrackerRequest.Name;
        trackerDto.Description = updateTrackerRequest.Description;

        await _trackerRepository.UpdateTrackerAsync(trackerDto, cancellationToken);

        return UpdateTrackerResponse.CreateSuccessful();
    }

    [HttpDelete("remove")]
    public async Task<RemoveTrackerResponse> RemoveTracker(int trackerId, CancellationToken cancellationToken)
    {
        var trackerDto =
            await _trackerRepository.GetTrackerByIdAsync(trackerId, cancellationToken);

        if (trackerDto == null)
        {
            return RemoveTrackerResponse.CreateUnsuccessful();
        }

        await _trackerRepository.RemoveTrackerAsync(trackerDto, cancellationToken);
        return RemoveTrackerResponse.CreateSuccessful();
    }

    [HttpGet("all")]
    public async Task<GetTrackersResponse> GetTrackers(int roomId, CancellationToken cancellationToken)
    {
        var possibleTrackers = await _trackerRepository.GetTrackersInRoomAsync(roomId, cancellationToken);

        if (possibleTrackers == null)
        {
            return GetTrackersResponse.CreateUnsuccessful();
        }

        var trackers =
            possibleTrackers.Select(t => new Tracker(t.TrackerDtoId, t.Name, t.Description, t.WifiMacAddress, t.BluetoothMacAddress, t.RoomDtoId));

        return GetTrackersResponse.CreateSuccessful(trackers);
    }

    [HttpGet("registration/info")]
    public async Task<GetTrackerRegistrationInfoResponse> GetTrackerRegistrationInfo(string wifiMacAddress,
        string bluetoothMacAddress, CancellationToken cancellationToken)
    {
        var possibleTracker =
            await _trackerRepository.GetTrackerByMacAddresses(wifiMacAddress, bluetoothMacAddress, cancellationToken);

        if (possibleTracker == null)
        {
            await _trackerRepository.AddUnregisteredTracker(wifiMacAddress, bluetoothMacAddress, cancellationToken);
            return GetTrackerRegistrationInfoResponse.CreateUnregistered();
        }

        return GetTrackerRegistrationInfoResponse.CreateRegistered(possibleTracker.TrackerDtoId);
    }

    [HttpGet("registration/unregistered")]
    public async Task<GetUnregisteredTrackersResponse> GetUnregisteredTrackers(CancellationToken cancellationToken)
    {
        var unregisteredTrackers = await _trackerRepository.GetUnregisteredTrackers(cancellationToken);

        return new GetUnregisteredTrackersResponse(
            unregisteredTrackers.Select(t => new UnregisteredTracker(t.WifiMacAddress, t.BluetoothMacAddress)));
    }
}
