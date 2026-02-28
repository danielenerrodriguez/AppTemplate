# Testing Strategy

## Overview
We test strategically -- thorough enough to catch bugs, light enough not to slow us down.

## What We Test

| Layer | Tool | Priority | What to Test |
|-------|------|----------|-------------|
| Services (business logic) | xUnit + FluentAssertions | HIGH | All public methods, edge cases, error handling |
| API endpoints | xUnit + WebApplicationFactory | MEDIUM | Status codes, response shapes |
| Blazor components | bUnit | MEDIUM | Rendering, user interactions |
| AI service wrapper | xUnit + NSubstitute | LOW | Your logic around the AI calls (mock the SDK) |
| Authentication | Skip | -- | Trust the framework |
| CSS/Styling | Skip | -- | Visual verification only |

## Test Project Structure
Tests mirror the source code structure:
```
tests/
  AppTemplate.Api.Tests/
    Features/
      Weather/
        WeatherServiceTests.cs      <-- mirrors Api/Features/Weather/WeatherService.cs
        WeatherEndpointsTests.cs     <-- mirrors Api/Features/Weather/WeatherEndpoints.cs
    Shared/
      AI/
        ClaudeServiceTests.cs        <-- mirrors Api/Shared/AI/ClaudeService.cs
  AppTemplate.Web.Tests/
    Features/
      Weather/
        WeatherPageTests.cs          <-- mirrors Web/Components/Features/Weather/WeatherPage.razor
```

## Naming Convention
```
MethodName_Scenario_ExpectedResult
```

Examples:
- `GetForecastAsync_ReturnsNonEmptyList`
- `GetForecastForCityAsync_ValidCity_ReturnsForecast`
- `GetForecastForCityAsync_InvalidCity_ReturnsNull`
- `SendMessageAsync_EmptyPrompt_ThrowsArgumentException`

## Test Patterns

### Service Tests (most common)
```csharp
public class WeatherServiceTests
{
    private readonly WeatherService _sut; // System Under Test

    public WeatherServiceTests()
    {
        _sut = new WeatherService();
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsNonEmptyList()
    {
        // Arrange (done in constructor)

        // Act
        var result = await _sut.GetForecastAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [InlineData("London")]
    [InlineData("New York")]
    public async Task GetForecastForCityAsync_ValidCity_ReturnsForecast(string city)
    {
        // Act
        var result = await _sut.GetForecastForCityAsync(city);

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be(city);
    }
}
```

### Mocking External Dependencies
```csharp
public class SomeServiceTests
{
    private readonly ISomeDependency _mockDependency;
    private readonly SomeService _sut;

    public SomeServiceTests()
    {
        _mockDependency = Substitute.For<ISomeDependency>();
        _sut = new SomeService(_mockDependency);
    }

    [Fact]
    public async Task DoWork_CallsDependency()
    {
        // Arrange
        _mockDependency.GetData().Returns("test data");

        // Act
        var result = await _sut.DoWork();

        // Assert
        result.Should().Contain("test data");
        await _mockDependency.Received(1).GetData();
    }
}
```

### Blazor Component Tests (bUnit)
```csharp
public class WeatherPageTests : TestContext
{
    [Fact]
    public void RendersLoadingState()
    {
        // Arrange
        Services.AddSingleton(Substitute.For<IWeatherService>());

        // Act
        var cut = RenderComponent<WeatherPage>();

        // Assert
        cut.Markup.Should().Contain("Loading");
    }
}
```

## Running Tests
```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/AppTemplate.Api.Tests

# Run tests matching a filter
dotnet test --filter "WeatherService"

# Run with coverage (if coverlet is configured)
dotnet test --collect:"XPlat Code Coverage"
```

## When to Write Tests
- ALWAYS: Before merging a feature to main
- ALWAYS: For any business logic in services
- NICE TO HAVE: For Blazor component rendering
- SKIP: For trivial DTOs, framework code, or UI styling

## Quick Test Checklist
Before merging to main:
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
- [ ] New service methods have at least one test
- [ ] Edge cases covered (null, empty, invalid input)
