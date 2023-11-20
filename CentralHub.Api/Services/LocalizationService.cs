using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationService : ILocalizationService
{
    private Thread MeasurementRemover;
    private Dictionary<int, MeasurementGroup> _measurements = new Dictionary<int, MeasurementGroup>();
    public void AddMeasurements(int id, IReadOnlyCollection<Measurement> measurements)
    {
        lock (_measurements)
        {
            if (_measurements.TryGetValue(id, out MeasurementGroup value))
            {
                value.AddMeasurements(measurements.ToList());
            }
            else
            {
                _measurements.Add(id, new MeasurementGroup(measurements.ToList()));
            }
        }
        if (MeasurementRemover == null || !(MeasurementRemover.ThreadState == ThreadState.Running))
        {
            MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
            MeasurementRemover.Start();
        }
        Console.WriteLine(_measurements.Values.First().Measurements.Count);
    }
    private void RemoveMeasurements()
    {
        while (true)
        {
            lock (_measurements)
            {
                foreach (var key in _measurements.Keys)
                {
                    foreach (var measurementKey in _measurements[key].Measurements.Keys)
                    {
                        if (DateTime.Now >= measurementKey + TimeSpan.FromMinutes(2))
                        {
                            _measurements[key].Measurements.Remove(measurementKey);
                        }
                    }
                }
                if (_measurements.Keys.All(key => _measurements[key].Measurements.Count == 0))
                {
                    break;
                }
                var delay = 120;
                foreach (var key in _measurements.Keys)
                {
                    foreach (var timestamp in _measurements[key].Measurements.Keys)
                    {
                        var new_delay = (timestamp + TimeSpan.FromMinutes(2) - DateTime.Now).TotalSeconds;
                        delay = new_delay < delay ? (int)new_delay : delay;
                    }
                }
                Task.Delay(delay);
            }
        }
    }
}