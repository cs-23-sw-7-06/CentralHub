
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CentralHub.Api.Model;

[Keyless]
public sealed class MeasurementCollection
{
    [Obsolete("Deserialization only")]
    public MeasurementCollection()
    {
    }

    public MeasurementCollection(int trackerId, ICollection<Measurement> measurements)
    {
        TrackerId = trackerId;
        Measurements = measurements;

    }

    public int TrackerId { get; set; }

    public ICollection<Measurement> Measurements { get; set; }
}