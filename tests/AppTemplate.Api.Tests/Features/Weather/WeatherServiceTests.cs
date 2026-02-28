using AppTemplate.Api.Features.Weather;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AppTemplate.Api.Tests.Features.Weather;

public class WeatherServiceTests
{
    private readonly WeatherService _sut;

    public WeatherServiceTests()
    {
        var logger = Substitute.For<ILogger<WeatherService>>();
        _sut = new WeatherService(logger);
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsNonEmptyList()
    {
        // Act
        var result = await _sut.GetForecastAsync();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsForecastForAllCities()
    {
        // Act
        var result = (await _sut.GetForecastAsync()).ToList();

        // Assert - there are 10 cities in CityTemperatureRanges
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsValidTemperatures()
    {
        // Act
        var result = await _sut.GetForecastAsync();

        // Assert
        result.Should().AllSatisfy(f =>
        {
            f.TemperatureC.Should().BeInRange(-20, 55);
        });
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsFutureDates()
    {
        // Act
        var result = await _sut.GetForecastAsync();

        // Assert
        var today = DateOnly.FromDateTime(DateTime.Now);
        result.Should().AllSatisfy(f =>
        {
            f.Date.Should().BeOnOrAfter(today);
        });
    }

    [Fact]
    public async Task GetForecastForCityAsync_KnownCity_ReturnsForecast()
    {
        // Act
        var result = await _sut.GetForecastForCityAsync("London");

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("London");
    }

    [Fact]
    public async Task GetForecastForCityAsync_KnownCity_ReturnsValidTemperature()
    {
        // Act
        var result = await _sut.GetForecastForCityAsync("Tokyo");

        // Assert
        result.Should().NotBeNull();
        result!.TemperatureC.Should().BeInRange(-20, 55);
    }

    [Fact]
    public async Task GetForecastForCityAsync_UnknownCity_ReturnsNull()
    {
        // Act
        var result = await _sut.GetForecastForCityAsync("UnknownCity");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetForecastForCityAsync_IsCaseInsensitive()
    {
        // Act
        var result = await _sut.GetForecastForCityAsync("london");

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().BeEquivalentTo("London");
    }

    [Fact]
    public void WeatherForecastDto_TemperatureF_CalculatesCorrectly()
    {
        // Arrange
        var forecast = new WeatherForecastDto(
            DateOnly.FromDateTime(DateTime.Now),
            100,
            "Hot",
            "TestCity");

        // Assert - 100C: 32 + (int)(100 * 9.0 / 5.0) = 32 + 180 = 212
        forecast.TemperatureF.Should().Be(212);
    }

    [Fact]
    public void WeatherForecastDto_DefaultCity_IsDefault()
    {
        // Arrange
        var forecast = new WeatherForecastDto(
            DateOnly.FromDateTime(DateTime.Now),
            20,
            "Nice");

        // Assert
        forecast.City.Should().Be("Default");
    }
}
