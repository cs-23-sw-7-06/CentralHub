using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface ILocalizationTargetService
{


    public void AddMeasurements(int id, List<Measurement> measurements);

    public List<Measurement> GetMeasurementsForId(int id, CancellationToken token);

}