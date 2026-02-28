namespace AppTemplate.Api.Features.Weather;

public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly Dictionary<string, (int MinTemp, int MaxTemp)> CityTemperatureRanges = new(StringComparer.OrdinalIgnoreCase)
    {
        ["New York"] = (-10, 35),
        ["Los Angeles"] = (10, 40),
        ["Chicago"] = (-15, 35),
        ["Miami"] = (15, 38),
        ["Seattle"] = (0, 30),
        ["Denver"] = (-10, 35),
        ["Phoenix"] = (5, 48),
        ["London"] = (-5, 30),
        ["Tokyo"] = (-2, 36),
        ["Sydney"] = (8, 42)
    };

    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<WeatherForecastDto>> GetForecastAsync()
    {
        _logger.LogInformation("Generating weather forecast for all cities");

        var forecasts = CityTemperatureRanges.Keys
            .Select(city => GenerateForecast(city, CityTemperatureRanges[city]))
            .ToList();

        return Task.FromResult<IEnumerable<WeatherForecastDto>>(forecasts);
    }

    public Task<WeatherForecastDto?> GetForecastForCityAsync(string city)
    {
        _logger.LogInformation("Generating weather forecast for city: {City}", city);

        if (CityTemperatureRanges.TryGetValue(city, out var range))
        {
            var forecast = GenerateForecast(city, range);
            return Task.FromResult<WeatherForecastDto?>(forecast);
        }

        _logger.LogWarning("City not found: {City}", city);
        return Task.FromResult<WeatherForecastDto?>(null);
    }

    private static WeatherForecastDto GenerateForecast(string city, (int MinTemp, int MaxTemp) range)
    {
        return new WeatherForecastDto(
            DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(1, 7))),
            Random.Shared.Next(range.MinTemp, range.MaxTemp),
            Summaries[Random.Shared.Next(Summaries.Length)],
            city);
    }
}
