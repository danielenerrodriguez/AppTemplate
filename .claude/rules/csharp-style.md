# C# Code Style

- Use file-scoped namespaces: `namespace AppTemplate.Api.Features.Weather;`
- Nullable reference types are enabled -- use `?` for nullable types, never use `!` suppress operator
- Use primary constructors for simple DI:
  ```csharp
  public class WeatherService(ILogger<WeatherService> logger) : IWeatherService
  ```
  But traditional constructors are fine for complex initialization
- Use records for DTOs and value objects: `public record WeatherForecastDto(DateOnly Date, int TemperatureC);`
- Use classes for services and anything with mutable state
- Prefer `var` for local variables when the type is obvious
- Use `async`/`await` for all I/O operations -- never use `.Result` or `.Wait()`
- Use expression-bodied members for simple one-liners
- String interpolation over concatenation: `$"Hello {name}"` not `"Hello " + name`
- Collection expressions where appropriate: `List<string> items = ["a", "b", "c"];`
- Pattern matching preferred over type casting
- Constants in PascalCase, private fields with underscore prefix: `private readonly ILogger _logger;`
- XML doc comments on public interfaces and service methods
