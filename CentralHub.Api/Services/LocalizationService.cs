using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationService : ILocalizationService
{
    private Thread MeasurementRemover;
    private Dictionary<int, List<Measurement>> _measurements = new Dictionary<int, List<Measurement>>();

    public LocalizationService()
    {
        MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
        MeasurementRemover.Start();
    }
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
    
    private void RemoveMeasurements()
    {
        while (true)
        {
            if (_measurements.Count == 0)
            {
                break;
            }

            foreach (var key in _measurements.Keys)
            {
                foreach (var measurement in _measurements[key])
                {
                    if (DateTime.Now >= DateTime.Now + TimeSpan.FromMinutes(2))
                    { // TODO: Change second datetime.now to datetime from addMeasurements when it has been implemented
                        _measurements[key].Remove(measurement);
                    }
                }
            }
        }
    }
}