using System.Net;
using System.Net.Http.Json;
using AppTemplate.Api.Shared.AI;
using FluentAssertions;
using NSubstitute;

namespace AppTemplate.Api.Tests.Features.Chat;

public class ChatEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ChatEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task PostChat_EmptyMessage_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/chat",
            new { Message = "", DeviceId = "dev-1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostChat_ValidMessage_ReturnsOkWithResponse()
    {
        _factory.MockAiService
            .SendMessageAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("Hello from AI");

        var response = await _client.PostAsJsonAsync("/api/chat",
            new { Message = "Hi", DeviceId = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ChatResponseResult>();
        result!.Response.Should().Be("Hello from AI");
    }

    [Fact]
    public async Task PostStream_EmptyMessage_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/chat/stream",
            new { Message = "", DeviceId = "dev-1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostStream_ValidMessage_ReturnsEventStream()
    {
        _factory.MockAiService
            .StreamMessageAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable("Hello", " World"));

        var response = await _client.PostAsJsonAsync("/api/chat/stream",
            new { Message = "Hi", DeviceId = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/event-stream");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("data: Hello");
        body.Should().Contain("data:  World");
        body.Should().Contain("data: [DONE]");
    }

    [Fact]
    public async Task GetHistory_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/chat/history/test-device");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().StartWith("["); // JSON array
    }

    [Fact]
    public async Task DeleteHistory_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/chat/history/test-device");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetModels_ReturnsOkWithModelsAndDefault()
    {
        _factory.MockAiService
            .ListModelsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new List<ModelInfoDto>
            {
                new("claude-sonnet-4-20250514", "Claude Sonnet 4", DateTime.UtcNow)
            });

        var response = await _client.GetAsync("/api/chat/models");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ModelsResult>();
        result!.Models.Should().NotBeEmpty();
        result.DefaultModel.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetModels_WhenServiceThrows_ReturnsEmptyListGracefully()
    {
        _factory.MockAiService
            .ListModelsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<List<ModelInfoDto>>(_ => throw new HttpRequestException("API down"));

        var response = await _client.GetAsync("/api/chat/models");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ModelsResult>();
        result!.Models.Should().BeEmpty();
        result.DefaultModel.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEnvKeyAvailable_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/chat/env-key-available");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EnvKeyResult>();
        result.Should().NotBeNull();
    }

    private static async IAsyncEnumerable<string> ToAsyncEnumerable(params string[] items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.CompletedTask;
        }
    }

    private record ChatResponseResult(string Response, DateTime Timestamp);
    private record ModelsResult(List<ModelItem> Models, string DefaultModel);
    private record ModelItem(string Id, string DisplayName);
    private record EnvKeyResult(bool Available);
}
