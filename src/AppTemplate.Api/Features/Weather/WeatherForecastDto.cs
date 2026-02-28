namespace AppTemplate.Api.Features.Weather;

public record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    string City = "Default")
{
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);
}
