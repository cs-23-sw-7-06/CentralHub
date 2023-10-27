using System.Text.Json;

namespace CentralHub.WebUI.Data;

public class WeatherForecastService
{
    private readonly IHttpClientFactory _clientFactory;

    public WeatherForecastService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public async Task<WeatherForecast[]> GetForecastAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "http://localhost:5003/WeatherForecast");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "CentralHub.WebUI");

        var client = _clientFactory.CreateClient();

        var response = await client.SendAsync(request);
        var forecast = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();

        if (forecast == null)
        {
            throw new InvalidOperationException("Shit brokey");
        }

        return forecast;
    }
}