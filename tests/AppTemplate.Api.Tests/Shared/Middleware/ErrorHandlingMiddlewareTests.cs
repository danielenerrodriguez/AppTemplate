using System.Net;
using System.Text.Json;
using AppTemplate.Api.Shared.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppTemplate.Api.Tests.Shared.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private static IHost CreateTestHost(RequestDelegate handler, string environment = "Development")
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseEnvironment(environment);
                webBuilder.ConfigureServices(services => services.AddRouting());
                webBuilder.Configure(app =>
                {
                    app.UseErrorHandling();
                    app.Run(handler);
                });
            })
            .Start();
    }

    [Fact]
    public async Task InvokeAsync_NoException_ReturnsSuccessfully()
    {
        using var host = CreateTestHost(async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("OK");
    }

    [Fact]
    public async Task InvokeAsync_Exception_Returns500WithProblemDetails()
    {
        using var host = CreateTestHost(_ => throw new InvalidOperationException("Something broke"));
        var client = host.GetTestClient();

        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("An internal server error occurred.");
        problem.Instance.Should().Be("/test");
    }

    [Fact]
    public async Task HandleException_Development_ExposesExceptionMessage()
    {
        using var host = CreateTestHost(
            _ => throw new InvalidOperationException("Detailed error info"),
            environment: "Development");
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/things");
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem!.Detail.Should().Be("Detailed error info");
    }

    [Fact]
    public async Task HandleException_Production_HidesExceptionMessage()
    {
        using var host = CreateTestHost(
            _ => throw new InvalidOperationException("Secret internal details"),
            environment: "Production");
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/things");
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        problem!.Detail.Should().Be("An unexpected error occurred. Please try again later.");
        problem.Detail.Should().NotContain("Secret");
    }
}
