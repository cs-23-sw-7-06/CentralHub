using System.Diagnostics.Metrics;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/room")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly ILocalizationTargetService _localizationTargetService;

    public RoomController(ILogger<RoomController> logger, IRoomRepository roomRepository, ILocalizationTargetService localizationTargetService)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _localizationTargetService = localizationTargetService;
    }

    [HttpPost("add")]
    public async Task AddRoom(Room room, CancellationToken cancellationToken)
    {
        await _roomRepository.AddRoomAsync(room, cancellationToken);
    }

    [HttpPut("update")]
    public async Task UpdateRoom(Room room, CancellationToken cancellationToken)
    {
        // TODO: Does this even work!?
        await _roomRepository.UpdateRoomAsync(room, cancellationToken);
    }

    [HttpDelete("remove")]
    public async Task RemoveRoom(int id, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomByIdAsync(id, cancellationToken);
        if (room == null)
        {
            _logger.LogError("Room did not exist!");
            return;
        }
        await _roomRepository.RemoveRoomAsync(room, cancellationToken);
    }

    [HttpGet("all")]
    public async Task<IEnumerable<Room>> GetRooms(CancellationToken cancellationToken)
    {
        return await _roomRepository.GetRoomsAsync(cancellationToken);
    }

    [HttpGet("get")]
    public async Task<Room?> GetRoom(int id, CancellationToken cancellationToken)
    {
        return await _roomRepository.GetRoomByIdAsync(id, cancellationToken);
    }

    [HttpGet("measurements")]
    public async Task<List<Measurement>> GetMeasurements(int id, CancellationToken token)
    {
        return await Task.FromResult(_localizationTargetService.GetMeasurementsForId(id, token));
    }
}
