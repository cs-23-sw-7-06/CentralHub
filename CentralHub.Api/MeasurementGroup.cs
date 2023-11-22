using CentralHub.Api.Model;

namespace CentralHub.Api;

public class MeasurementGroup
{
    public MeasurementGroup(List<Measurement> measurements)
    {
        Timestamp = DateTime.Now;
        Measurements = measurements.ToList();
    }
    public DateTime Timestamp { get; }
    public List<Measurement> Measurements { get; }
}