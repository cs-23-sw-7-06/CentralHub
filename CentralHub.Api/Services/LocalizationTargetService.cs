using CentralHub.Api.Model;

namespace CentralHub.Api.Services;

public class LocalizationTargetService : ILocalizationTargetService
{
    private Dictionary<int, List<Measurement>> Measurements = new Dictionary<int, List<Measurement>>();

    private Thread MeasurementRemover;
    private bool MeasurementRemoverRunning;

    public LocalizationTargetService()
    {
        MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
        MeasurementRemover.Start();
    }

    public void AddMeasurements(int id, List<Measurement> measurements)
    {
        if (!(MeasurementRemover.ThreadState == ThreadState.Running))
        {
            MeasurementRemover = new Thread(new ThreadStart(RemoveMeasurements));
        }
        lock (Measurements)
        {
            if (Measurements.ContainsKey(id))
            {
                Measurements[id].AddRange(measurements);
            }
            else
            {
                Measurements.Add(id, measurements.ToList());
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

    private void RemoveMeasurements()
    {
        while (true)
        {
            if (Measurements.Count == 0)
            {
                break;
            }

            foreach (var key in Measurements.Keys)
            {
                foreach (var measurement in Measurements[key])
                {
                    if (DateTime.Now >= DateTime.Now + TimeSpan.FromMinutes(2))
                    { // TODO: Change second datetime.now to datetime from addMeasurements when it has been implemented
                        Measurements[key].Remove(measurement);
                    }
                }
            }
        }
    }

}