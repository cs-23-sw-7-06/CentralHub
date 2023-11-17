using CentralHub.Api.Dtos;
using CentralHub.Api.Model;
using CentralHub.Api.Model.Requests;
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
            MacAddress = addTrackerRequest.MacAddress,
            RoomDtoId = room.RoomDtoId,
            RoomDto = room
        };

        var trackerId = await _trackerRepository.AddTrackerAsync(trackerDto, cancellationToken);

        return AddTrackerResponse.CreateSuccessful(trackerId);
    }

    [HttpPut("update")]
    public async Task UpdateTracker(UpdateTrackerRequest updateTrackerRequest, CancellationToken cancellationToken)
    {
        // TODO: Report error
        var trackerDto = await _trackerRepository.GetTrackerAsync(updateTrackerRequest.TrackerId, cancellationToken);

        trackerDto.Name = updateTrackerRequest.Name;
        trackerDto.Description = updateTrackerRequest.Description;

        await _trackerRepository.UpdateTrackerAsync(trackerDto, cancellationToken);
    }

    [HttpDelete("remove")]
    public async Task RemoveTracker(int trackerId, CancellationToken cancellationToken)
    {
        var trackerDto =
            await _trackerRepository.GetTrackerAsync(trackerId, cancellationToken);

        if (trackerDto != null)
        {
            // TODO: Report an error if no tracker was found
            await _trackerRepository.RemoveTrackerAsync(trackerDto, cancellationToken);
        }
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
            possibleTrackers.Select(t => new Tracker(t.TrackerDtoId, t.Name, t.Description, t.MacAddress, t.RoomDtoId));

        return GetTrackersResponse.CreateSuccessful(trackers);
    }
}
