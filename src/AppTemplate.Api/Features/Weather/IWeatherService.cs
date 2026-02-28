namespace AppTemplate.Api.Features.Weather;

/// <summary>
/// Provides weather forecast data. This is the example feature demonstrating
/// the vertical slice pattern — teams can use it as a template for new features.
/// </summary>
public interface IWeatherService
{
    /// <summary>Returns forecasts for all known cities.</summary>
    Task<IEnumerable<WeatherForecastDto>> GetForecastAsync();

    /// <summary>
    /// Returns a forecast for a specific city, or <c>null</c> if the city is not found.
    /// </summary>
    Task<WeatherForecastDto?> GetForecastForCityAsync(string city);
}
