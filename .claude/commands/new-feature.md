Scaffold a new vertical slice feature named "$ARGUMENTS".

Do the following steps:

1. Create the API feature folder: `src/AppTemplate.Api/Features/{Name}/`
   - `I{Name}Service.cs` -- interface with CRUD methods (GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync)
   - `{Name}Service.cs` -- implementation class. Use `AppDbContext` for persistence if the feature needs data storage. Inject via constructor.
   - `{Name}Endpoints.cs` -- Minimal API endpoints (GET /, GET /{id}, POST /, PUT /{id}, DELETE /{id}) mapped to a RouteGroupBuilder. Wrap responses in `ApiResponse<T>`.
   - `{Name}Dto.cs` -- record DTOs ({Name}Dto, Create{Name}Dto, Update{Name}Dto) with sensible placeholder properties (Id, Name, Description, CreatedAt)

2. If the feature needs database storage:
   - Add entity class to `src/AppTemplate.Api/Shared/Data/` (following the pattern in `ChatEntities.cs`)
   - Add DbSet to `AppDbContext.cs`
   - Add entity configuration in `OnModelCreating`

3. Create the Blazor feature folder: `src/AppTemplate.Web/Components/Features/{Name}/`
   - `{Name}Page.razor` -- page with @page "/{kebab-name}", use MudBlazor components (MudTable, MudButton, MudCard, etc.), loading state, error handling
   - `{Name}Page.razor.cs` -- code-behind with HttpClient injection, OnInitializedAsync, CRUD methods

4. Register the service in `src/AppTemplate.Api/Shared/Extensions/ServiceCollectionExtensions.cs`:
   - Add `public static IServiceCollection Add{Name}Feature(this IServiceCollection services)` method
   - Register the service as Scoped

5. Map the endpoints in `src/AppTemplate.Api/Program.cs`:
   - Add `app.MapGroup("/api/{kebab-name}").Map{Name}Endpoints().WithTags("{Name}");`

6. Add the page to navigation in `src/AppTemplate.Web/Components/Layout/NavMenu.razor`

7. Add the using to `src/AppTemplate.Web/Components/_Imports.razor`

8. Create test file: `tests/AppTemplate.Api.Tests/Features/{Name}/{Name}ServiceTests.cs`
   - At least 5 unit tests covering the CRUD operations
   - Use FluentAssertions and NSubstitute
   - If using AppDbContext, use `Microsoft.EntityFrameworkCore.InMemory` for test database

9. Run `dotnet build` to verify everything compiles
10. Run `dotnet test` to verify tests pass
11. Update `CLAUDE.md`:
    - Add the feature to the "Pre-built Features" section
    - Update any relevant architecture notes
12. Update `README.md`:
    - Add the feature to the features list
13. Commit with message `feat: scaffold {Name} feature with CRUD endpoints, Blazor page, and tests`

Use the Weather feature as the pattern to follow for code style, naming, and structure.
Use the Chat feature as a reference for database persistence and AppDbContext usage.
