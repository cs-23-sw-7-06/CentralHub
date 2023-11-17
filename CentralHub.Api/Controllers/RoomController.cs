using System.Diagnostics.Metrics;
using CentralHub.Api.Dtos;
using CentralHub.Api.Model.Requests.Room;
using CentralHub.Api.Model.Responses.Room;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/room")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly ILocalizationService _localizationService;

    public RoomController(ILogger<RoomController> logger, IRoomRepository roomRepository, ILocalizationService localizationService)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _localizationService = localizationService;
    }

    [HttpPost("add")]
    public async Task<AddRoomResponse> AddRoom(AddRoomRequest addRoomRequest, CancellationToken cancellationToken)
    {
        var roomDto = new RoomDto()
        {
            Name = addRoomRequest.Name,
            Description = addRoomRequest.Description
        };

        var roomId = await _roomRepository.AddRoomAsync(roomDto, cancellationToken);

        return new AddRoomResponse(roomId);
    }

    [HttpPut("update")]
    public async Task UpdateRoom(UpdateRoomRequest updateRoomRequest, CancellationToken cancellationToken)
    {
        var roomDto = await _roomRepository.GetRoomByIdAsync(updateRoomRequest.RoomId, cancellationToken);
        if (roomDto == null)
        {
            // TODO: Report error
            throw new InvalidOperationException("roomDto was null");
        }

        roomDto.Name = updateRoomRequest.Name;
        roomDto.Description = updateRoomRequest.Description;

        await _roomRepository.UpdateRoomAsync(roomDto, cancellationToken);
    }

    [HttpDelete("remove")]
    public async Task RemoveRoom(int roomId, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomByIdAsync(roomId, cancellationToken);
        if (room == null)
        {
            // TODO: Report error
            _logger.LogError("Room did not exist!");
            return;
        }
        await _roomRepository.RemoveRoomAsync(room, cancellationToken);
    }

    [HttpGet("all")]
    public async Task<IEnumerable<Room>> GetRooms(CancellationToken cancellationToken)
    {
        var roomDtos = await _roomRepository.GetRoomsAsync(cancellationToken);
        return roomDtos.Select(r => new Room(r.RoomDtoId, r.Name, r.Description));
    }

    [HttpGet("get")]
    public async Task<GetRoomResponse> GetRoom(int id, CancellationToken cancellationToken)
    {
        var roomDto = await _roomRepository.GetRoomByIdAsync(id, cancellationToken);

        if (roomDto == null)
        {
            return GetRoomResponse.CreateUnsuccessful();
        }

        var room = new Room(
            roomDto.RoomDtoId,
            roomDto.Name,
            roomDto.Description);

        return GetRoomResponse.CreateSuccessful(room);
    }
}
