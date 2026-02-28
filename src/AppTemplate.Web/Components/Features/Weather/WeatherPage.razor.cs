using Microsoft.AspNetCore.Components;

namespace AppTemplate.Web.Components.Features.Weather;

public partial class WeatherPage : ComponentBase
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    private WeatherForecastDto[]? _forecasts;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadForecast();
    }

    private async Task LoadForecast()
    {
        try
        {
            _errorMessage = null;
            _forecasts = await Http.GetFromJsonAsync<WeatherForecastDto[]>("api/weather");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load weather data: {ex.Message}";
            _forecasts = null;
        }
    }
}

// Note: This DTO is intentionally duplicated from AppTemplate.Api.Features.Weather.WeatherForecastDto
// to avoid a cross-project dependency (Api project uses Microsoft.NET.Sdk.Web which causes conflicts).
// Keep the two in sync manually — or extract to a shared class library if duplication becomes a problem.
public record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    string City = "Default")
{
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);
}
