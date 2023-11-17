using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationService : ILocalizationService
{
    private Dictionary<int, List<Measurement>> _measurements = new Dictionary<int, List<Measurement>>();

    public void AddMeasurements(int id, IReadOnlyCollection<Measurement> measurements)
    {
        lock (_measurements)
        {
            if (_measurements.TryGetValue(id, out List<Measurement> value))
            {
                value.AddRange(measurements);
            }
            else
            {
                _measurements.Add(id, measurements.ToList());
            }
        }
    }
}