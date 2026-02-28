# Minimal API Rules

- Group endpoints by feature using `MapGroup`:
  ```csharp
  app.MapGroup("/api/weather").MapWeatherEndpoints();
  ```
- Use `TypedResults` for return types:
  ```csharp
  TypedResults.Ok(data)
  TypedResults.NotFound()
  TypedResults.BadRequest("message")
  TypedResults.Created($"/api/items/{id}", item)
  ```
- Always validate input -- check for null, empty strings, invalid ranges
- Return proper HTTP status codes:
  - 200: Success (GET, PUT)
  - 201: Created (POST)
  - 204: No Content (DELETE)
  - 400: Bad Request (validation failure)
  - 404: Not Found
  - 500: Internal Server Error (caught by middleware)
- Use dependency injection in endpoint handlers via parameters
- Endpoint extension methods return `RouteGroupBuilder` for chaining
- Name endpoints with `.WithName()` for OpenAPI
- Add `.WithTags()` for Swagger grouping
- Keep endpoint handlers thin -- delegate to services for business logic
