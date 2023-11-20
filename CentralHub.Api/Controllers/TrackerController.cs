using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/tracker")]
public class TrackerController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILocalizationTargetService _localizationTargetService;

    public TrackerController(IRoomRepository roomRepository, ILocalizationTargetService localizationTargetService)
    {
        _roomRepository = roomRepository;
        _localizationTargetService = localizationTargetService;
    }

    [HttpGet("all")]
    public async Task<IEnumerable<Tracker>> GetTrackers(int id, CancellationToken cancellationToken)
    {
        return (await _roomRepository.GetRoomByIdAsync(id, cancellationToken))?.Trackers ?? Enumerable.Empty<Tracker>();
    }

    [HttpPost("add")]
    public async Task AddTracker(Tracker tracker, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomByIdAsync(tracker.RoomId, cancellationToken);
        if (room == null)
        {
            throw new InvalidOperationException($"Room with id {tracker.RoomId} was not found");
        }

        // Room is not included in the json so we need to insert it here.
        tracker.Room = room;

        await _roomRepository.AddTrackerAsync(tracker, cancellationToken);
    }

    [HttpPut("update")]
    public async Task UpdateTracker(Tracker tracker, CancellationToken cancellationToken)
    {
        // TODO: Should we support reparenting trackers?
        await _roomRepository.UpdateTrackerAsync(tracker, cancellationToken);
    }

    [HttpDelete("remove")]
    public async Task RemoveTracker(Tracker tracker, CancellationToken cancellationToken)
    {
        await _roomRepository.RemoveTrackerAsync(tracker, cancellationToken);
    }

    [HttpPost("measurement")]
    public async Task PostMeasurement(int id, List<Measurement> measurements, CancellationToken token)
    {
        _localizationTargetService.AddMeasurements(id, measurements);
    }
}
