# Testing Rules

- Use xUnit as the test framework
- Use FluentAssertions for all assertions -- never use `Assert.Equal()`, use `.Should().Be()`
- Use NSubstitute for mocking -- never create manual mock classes
- Test naming: `MethodName_Scenario_ExpectedResult`
- One assertion per test when practical (related assertions in a group are OK)
- Use `[Fact]` for single tests, `[Theory]` with `[InlineData]` for parameterized tests
- Mock all external dependencies (API clients, databases, file system)
- Test files must mirror source structure:
  `tests/AppTemplate.Api.Tests/Features/Weather/WeatherServiceTests.cs`
- Each test class gets a `_sut` field (System Under Test)
- Use Arrange-Act-Assert pattern with comments only if the test is complex
- Blazor component tests use bUnit's `TestContext` base class
- Never test framework/library code -- focus on YOUR logic
