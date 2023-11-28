using CentralHub.Api.Model.Requests.Localization;
using CentralHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralHub.Api.Controllers;

[ApiController]
[Route("/localization")]
public sealed class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpPost("measurements/add")]
    public Task AddMeasurements(AddMeasurementsRequest addMeasurementsRequest, CancellationToken cancellationToken)
    {
        _localizationService.AddMeasurements(
            addMeasurementsRequest.TrackerId,
            addMeasurementsRequest.Measurements);

        return Task.CompletedTask;
    }
}