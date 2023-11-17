using System.Text.Json.Serialization;

namespace CentralHub.Api.Model.Requests.Localization;

public sealed class AddMeasurementsRequest
{
    [JsonConstructor]
    public AddMeasurementsRequest(int trackerId, IReadOnlyCollection<Measurement> measurements)
    {
        TrackerId = trackerId;
        Measurements = measurements;
    }

    public int TrackerId { get; }

    public IReadOnlyCollection<Measurement> Measurements { get; }
}