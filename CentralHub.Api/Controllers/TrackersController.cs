using CentralHub.Api.DbContexts;
using CentralHub.Api.Model;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TrackersController : ControllerBase
{
    private readonly ITrackersRepository _trackersRepository;
    public TrackersController(ITrackersRepository trackersRepository)
    {
        _trackersRepository = trackersRepository;
    }

    [HttpGet(Name = "GetTrackers")]
    public async Task<IEnumerable<Tracker>> Get()
    {
        return await _trackersRepository.GetTrackersAsync();
    }

    [HttpPost(Name = "PostTracker")]
    public async Task Post(Tracker tracker)
    {
        await _trackersRepository.AddTracker(tracker);
    }
}
