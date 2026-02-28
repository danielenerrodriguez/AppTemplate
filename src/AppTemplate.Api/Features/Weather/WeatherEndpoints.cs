namespace AppTemplate.Api.Features.Weather;

public static class WeatherEndpoints
{
    public static RouteGroupBuilder MapWeatherEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IWeatherService weatherService) =>
        {
            var forecasts = await weatherService.GetForecastAsync();
            return TypedResults.Ok(forecasts);
        }).WithName("GetWeatherForecast");

        group.MapGet("/{city}", async (string city, IWeatherService weatherService) =>
        {
            var forecast = await weatherService.GetForecastForCityAsync(city);
            return forecast is not null
                ? TypedResults.Ok(forecast)
                : Results.NotFound();
        }).WithName("GetWeatherForCity");

        return group;
    }
}
