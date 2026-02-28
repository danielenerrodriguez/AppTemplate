using AppTemplate.Web.Components.Features.Weather;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AppTemplate.Web.Tests.Features.Weather;

public class WeatherPageTests : BunitContext
{
    public WeatherPageTests()
    {
        Services.AddMudServices();
    }

    private static HttpClient CreateMockHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        var mockHandler = new MockHandler(handler);
        return new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost") };
    }

    private static HttpClient CreateJsonHttpClient<T>(T responseBody)
    {
        return CreateMockHttpClient(_ =>
        {
            var json = JsonSerializer.Serialize(responseBody);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });
    }

    [Fact]
    public void WeatherPage_InitialRender_ShowsLoadingIndicator()
    {
        // Arrange - request that never completes so we stay in loading state
        var client = CreateMockHttpClient(_ =>
            new TaskCompletionSource<HttpResponseMessage>().Task);
        Services.AddSingleton(client);

        // Act
        var cut = Render<WeatherPage>();

        // Assert — MudProgressCircular renders a circular progress indicator
        cut.Markup.Should().Contain("mud-progress-circular");
    }

    [Fact]
    public void WeatherPage_AfterDataLoads_ShowsTable()
    {
        // Arrange
        var forecasts = new[]
        {
            new WeatherForecastDto(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 22, "Warm", "London"),
            new WeatherForecastDto(DateOnly.FromDateTime(DateTime.Now.AddDays(2)), 18, "Cool", "Tokyo")
        };
        Services.AddSingleton(CreateJsonHttpClient(forecasts));

        // Act
        var cut = Render<WeatherPage>();
        cut.WaitForState(() => cut.Markup.Contains("London"));

        // Assert
        cut.Markup.Should().Contain("London");
        cut.Markup.Should().Contain("Tokyo");
        cut.Markup.Should().Contain("Weather Forecast");
    }

    [Fact]
    public void WeatherPage_WhenApiFails_ShowsErrorMessage()
    {
        // Arrange
        var client = CreateMockHttpClient(_ =>
            throw new HttpRequestException("Connection refused"));
        Services.AddSingleton(client);

        // Act
        var cut = Render<WeatherPage>();
        cut.WaitForState(() => cut.Markup.Contains("mud-alert"));

        // Assert — MudAlert renders with mud-alert CSS class
        cut.Markup.Should().Contain("Failed to load weather data");
    }

    [Fact]
    public void WeatherPage_ShowsCorrectTableHeaders()
    {
        // Arrange
        var forecasts = new[]
        {
            new WeatherForecastDto(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 25, "Warm", "Miami")
        };
        Services.AddSingleton(CreateJsonHttpClient(forecasts));

        // Act
        var cut = Render<WeatherPage>();
        cut.WaitForState(() => cut.Markup.Contains("Miami"));

        // Assert
        cut.Markup.Should().Contain("Date");
        cut.Markup.Should().Contain("City");
        cut.Markup.Should().Contain("Summary");
    }

    [Fact]
    public void WeatherPage_RefreshButton_IsPresent()
    {
        // Arrange
        var forecasts = new[]
        {
            new WeatherForecastDto(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), 25, "Warm", "Miami")
        };
        Services.AddSingleton(CreateJsonHttpClient(forecasts));

        // Act
        var cut = Render<WeatherPage>();
        cut.WaitForState(() => cut.Markup.Contains("Refresh"));

        // Assert — MudButton renders with mud-button-root CSS class
        cut.Find("button.mud-button-root").TextContent.Should().Contain("Refresh");
    }

    /// <summary>
    /// Simple HttpMessageHandler that delegates to a provided function.
    /// </summary>
    private class MockHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public MockHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}
