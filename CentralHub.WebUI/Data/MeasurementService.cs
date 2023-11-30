using System.Globalization;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using CentralHub.Api.Model.Responses.AggregatedMeasurements;

namespace CentralHub.WebUI.Data;

public sealed class MeasurementService
{
    private readonly IHttpClientFactory _clientFactory;

    public MeasurementService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IReadOnlyCollection<AggregatedMeasurements>> GetAggregatedMeasurements(int roomId, DateTime timeStart, DateTime timeEnd,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"http://localhost:8081/measurements/range"
            + $"?roomId={roomId}"
            + $"&timeStart={UrlEncoder.Default.Encode(timeStart.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture))}"
            + $"&timeEnd={UrlEncoder.Default.Encode(timeEnd.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture))}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Somethings fucky");
        }

        var getAggregatedMeasurementsResponse = await response.Content.ReadFromJsonAsync<GetAggregatedMeasurementsResponse>(cancellationToken: cancellationToken);

        if (getAggregatedMeasurementsResponse == null || !getAggregatedMeasurementsResponse.Success)
        {
            throw new InvalidOperationException("Unsuccessful!");
        }

        return getAggregatedMeasurementsResponse.AggregatedMeasurements!;
    }
}