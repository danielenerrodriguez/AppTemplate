using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace AppTemplate.Api.Tests.Features.Weather;

public class WeatherEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;

    public WeatherEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task GetWeather_ReturnsOkWithForecasts()
    {
        var response = await _client.GetAsync("/api/weather");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherResult>>();
        forecasts.Should().NotBeEmpty();
        forecasts.Should().HaveCount(10); // 10 cities
    }

    [Fact]
    public async Task GetWeatherForCity_KnownCity_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/weather/London");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var forecast = await response.Content.ReadFromJsonAsync<WeatherResult>();
        forecast!.City.Should().BeEquivalentTo("London");
    }

    [Fact]
    public async Task GetWeatherForCity_UnknownCity_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/weather/Atlantis");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record WeatherResult(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary, string City);
}
