using System.Collections.Immutable;
using CentralHub.Api.Model;

namespace CentralHub.Api;

public class MeasurementGroup
{
    public MeasurementGroup(IEnumerable<Measurement> measurements)
    {
        Timestamp = DateTime.UtcNow;
        Measurements = measurements.ToImmutableArray();
    }
    public DateTime Timestamp { get; }
    public IReadOnlyList<Measurement> Measurements { get; }
}