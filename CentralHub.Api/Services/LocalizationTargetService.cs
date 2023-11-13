using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationTargetService : ILocalizationTargetService
{
    private Dictionary<int, List<Measurement>> Measurements = new Dictionary<int, List<Measurement>>();

    public void AddMeasurements(int id, List<Measurement> measurements)
    {
        lock (Measurements)
        {
            if (Measurements.ContainsKey(id))
            {
                Measurements[id].AddRange(measurements);
            }
            else
            {
                Measurements.Add(id, measurements);
            }
        }
    }

    public List<Measurement> GetMeasurementsForId(int id, CancellationToken token)
    {
        List<Measurement> measurements;
        lock (Measurements)
        {
            measurements = new List<Measurement>(Measurements.GetValueOrDefault(id, new List<Measurement>()));
        }
        return measurements;
    }

}