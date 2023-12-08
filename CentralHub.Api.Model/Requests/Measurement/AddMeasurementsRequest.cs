using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Measurement;

[method: JsonConstructor]
public sealed class AddMeasurementsRequest(
    int trackerId,
    IReadOnlyCollection<Responses.Measurement.Measurement> measurements)
{
    public int TrackerId { get; } = trackerId;

    public IReadOnlyCollection<Responses.Measurement.Measurement> Measurements { get; } = measurements;
}