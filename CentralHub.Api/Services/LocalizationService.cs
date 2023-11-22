using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationService : ILocalizationService
{
    private Thread? MeasurementRemover;
    private readonly List<ThreadState> validStates = new List<ThreadState>() { ThreadState.Running, ThreadState.WaitSleepJoin };
    private Dictionary<int, List<MeasurementGroup>> _measurements = new Dictionary<int, List<MeasurementGroup>>();
    public void AddMeasurements(int id, IReadOnlyCollection<Measurement> measurements)
    {
        lock (_measurements)
        {
            if (_measurements.TryGetValue(id, out List<MeasurementGroup> value))
            {
                value.Add(new MeasurementGroup(measurements.ToList()));
            }
            else
            {
                _measurements.Add(id, new List<MeasurementGroup>() { new MeasurementGroup(measurements.ToList()) });
            }
        }
        if (MeasurementRemover == null || !validStates.Contains(MeasurementRemover.ThreadState))
        {
            MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
            MeasurementRemover.Start();
        }
    }
    private void RemoveMeasurements()
    {
        var toBeRemoved = new List<MeasurementGroup>();
        while (true)
        {
            var delay = 120;
            lock (_measurements)
            {
                foreach (var key in _measurements.Keys)
                {
                    toBeRemoved.Clear();
                    foreach (var group in _measurements[key])
                    {
                        if (DateTime.Now >= group.Timestamp + TimeSpan.FromMinutes(2))
                        {
                            toBeRemoved.Add(group);
                        }
                        else
                        {
                            var new_delay = (group.Timestamp + TimeSpan.FromMinutes(2) - DateTime.Now).TotalSeconds;
                            delay = new_delay < delay ? (int)new_delay : delay;

                        }
                    }
                    foreach (var group in toBeRemoved)
                    {
                        _measurements[key].Remove(group);
                    }
                }
                if (_measurements.Keys.All(key => _measurements[key].Count == 0))
                {
                    return;
                }
            }
            Thread.Sleep(delay * 1000);
        }
    }
}