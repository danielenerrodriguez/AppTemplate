# Blazor Component Rules

- Always use code-behind pattern: `.razor` for markup, `.razor.cs` for logic
- Code-behind class must be `partial` and match the component name:
  ```csharp
  public partial class WeatherPage : ComponentBase
  ```
- Use `[Parameter]` for component inputs, `EventCallback` for outputs
- Use `@inject` in .razor for service injection
- Keep components small -- extract reusable parts to `Shared/Components/`
- Use `OnInitializedAsync` for data loading, not the constructor
- Handle loading states: show "Loading..." while awaiting data
- Handle error states: wrap data fetching in try/catch, show user-friendly errors
- Use `NavigationManager` for programmatic navigation
- Feature pages go in `Components/Features/{FeatureName}/`
- Shared/reusable components go in `Components/Shared/`
- Layout components go in `Components/Layout/`
