using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface ILocalizationService
{
    public void AddMeasurements(int id, IReadOnlyCollection<Measurement> measurements);
}