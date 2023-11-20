using CentralHub.Api.Model;

public class MeasurementGroup
{
    public MeasurementGroup(List<Measurement> measurements)
    {
        Measurements = new Dictionary<DateTime, Measurement>();
        foreach(var measurement in measurements){
            Measurements.Add(DateTime.Now, measurement);
        }
    }
    public void AddMeasurements(List<Measurement> measurements){
        foreach(var measurement in measurements){
            Measurements.Add(DateTime.Now, measurement);
        }
    }

    public Dictionary<DateTime, Measurement> Measurements {get; set;}
}