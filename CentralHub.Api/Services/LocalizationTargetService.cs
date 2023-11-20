using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationTargetService : ILocalizationTargetService
{
    private Dictionary<int, MeasurementGroup> Measurements = new();

    public Thread MeasurementRemover;
    private bool MeasurementRemoverRunning;

    public LocalizationTargetService()
    {
        MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
        MeasurementRemover.Start();
    }

    public void AddMeasurements(int id, List<Measurement> measurements)
    {
        lock (Measurements)
        {
            if (Measurements.ContainsKey(id))
            {
                Measurements[id].AddMeasurements(measurements);
            }
            else
            {
                Measurements.Add(id, new MeasurementGroup(measurements));
            }
        }
        if (!(MeasurementRemover.ThreadState == ThreadState.Running))
        {
            MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
            MeasurementRemover.Start();
        }
    }

    public MeasurementGroup GetMeasurementsForId(int id, CancellationToken token)
    {
        MeasurementGroup measurements;
        lock (Measurements)
        {
            measurements = Measurements.GetValueOrDefault(id, new MeasurementGroup(new List<Measurement>()));
        }
        return measurements;
    }

    private void RemoveMeasurements()
    {
        while (true)
        {
            lock(Measurements){
                foreach (var key in Measurements.Keys)
                {
                    foreach (var measurementKey in Measurements[key].Measurements.Keys)
                    {
                        if (DateTime.Now >= measurementKey + TimeSpan.FromMinutes(2))
                        {
                            Measurements[key].Measurements.Remove(measurementKey);
                        }
                    }
                }

                if (Measurements.Keys.All(key => Measurements[key].Measurements.Count == 0))
                {
                    break;
                }
                Task.Delay(Measurements.Keys.Select(key => Measurements[key].Measurements.Keys.Select(innerKey => ((innerKey+TimeSpan.FromMinutes(2)) - DateTime.Now).Seconds).Min()).Min());
            }
        }
    }
}