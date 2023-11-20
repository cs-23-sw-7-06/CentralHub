using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public interface ILocalizationTargetService
{

    public void AddMeasurements(int id, List<Measurement> measurements);

    public MeasurementGroup GetMeasurementsForId(int id, CancellationToken token);

}