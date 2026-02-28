using AppTemplate.Api.Features.Chat;
using AppTemplate.Api.Shared.AI;
using AppTemplate.Api.Shared.Auth;
using AppTemplate.Api.Shared.Data;
using AppTemplate.Api.Shared.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AppTemplate.Api.Tests.Features.Chat;

public class ChatServiceTests : IDisposable
{
    private readonly IAIService _mockAiService;
    private readonly ILogger<ChatService> _mockLogger;
    private readonly IConfiguration _mockConfiguration;
    private readonly IApiKeyProtector _mockProtector;
    private readonly AppDbContext _dbContext;
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _mockAiService = Substitute.For<IAIService>();
        _mockLogger = Substitute.For<ILogger<ChatService>>();

        // Passthrough protector for tests (no actual encryption)
        _mockProtector = Substitute.For<IApiKeyProtector>();
        _mockProtector.Protect(Arg.Any<string>()).Returns(c => c.Arg<string>());
        _mockProtector.Unprotect(Arg.Any<string>()).Returns(c => c.Arg<string>());

        // Configure a fallback API key so ResolveApiKeyAsync finds a key via config
        // (tests without a stored per-device key will use the default client path)
        var configData = new Dictionary<string, string?>
        {
            ["Anthropic:ApiKey"] = "sk-ant-test-fallback"
        };
        _mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _sut = new ChatService(_mockAiService, _dbContext, _mockConfiguration, _mockProtector, _mockLogger);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SendMessageAsync_WithValidMessage_ReturnsResponse()
    {
        // Arrange
        var request = new ChatRequestDto("Hello");
        _mockAiService.SendMessageAsync("Hello", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("Hi there!");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("Hi there!");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SendMessageAsync_WithSystemPrompt_BuildsPromptCorrectly()
    {
        // Arrange
        var request = new ChatRequestDto("Hello", SystemPrompt: "Be helpful");
        _mockAiService.SendMessageAsync(
            "System: Be helpful\n\nUser: Hello",
            Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns("I'm here to help!");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("I'm here to help!");
    }

    [Fact]
    public async Task SendMessageAsync_WithNullSystemPrompt_UsesMessageOnly()
    {
        // Arrange
        var request = new ChatRequestDto("What is the weather?");
        _mockAiService.SendMessageAsync("What is the weather?", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("I don't have weather data.");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("I don't have weather data.");
        await _mockAiService.Received(1).SendMessageAsync("What is the weather?", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WithEmptySystemPrompt_UsesMessageOnly()
    {
        // Arrange
        var request = new ChatRequestDto("Test message", SystemPrompt: "  ");
        _mockAiService.SendMessageAsync("Test message", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("Response");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("Response");
        await _mockAiService.Received(1).SendMessageAsync("Test message", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_WhenAiServiceThrows_PropagatesException()
    {
        // Arrange
        var request = new ChatRequestDto("Hello");
        _mockAiService.SendMessageAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<string>(_ => throw new HttpRequestException("API unavailable"));

        // Act
        var act = () => _sut.SendMessageAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("API unavailable");
    }

    [Fact]
    public void StreamMessageAsync_WithValidMessage_CallsAiService()
    {
        // Arrange
        var request = new ChatRequestDto("Stream this");
        _mockAiService.StreamMessageAsync("Stream this", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable(["chunk1", "chunk2"]));

        // Act
        var stream = _sut.StreamMessageAsync(request);

        // Assert
        stream.Should().NotBeNull();
    }

    [Fact]
    public async Task StreamMessageAsync_WithSystemPrompt_BuildsPromptCorrectly()
    {
        // Arrange
        var request = new ChatRequestDto("Hello", SystemPrompt: "Be concise");
        var expectedPrompt = "System: Be concise\n\nUser: Hello";
        _mockAiService.StreamMessageAsync(expectedPrompt, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable(["Hi"]));

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in _sut.StreamMessageAsync(request))
        {
            chunks.Add(chunk);
        }

        // Assert
        chunks.Should().ContainSingle().Which.Should().Be("Hi");
    }

    [Fact]
    public async Task SendMessageAsync_WithDeviceId_PersistsMessages()
    {
        // Arrange
        var deviceId = "test-device-123";
        var request = new ChatRequestDto("Hello", DeviceId: deviceId);
        _mockAiService.SendMessageAsync("Hello", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("Hi there!");

        // Act
        await _sut.SendMessageAsync(request);

        // Assert
        var messages = await _dbContext.ChatMessages
            .Where(m => m.DeviceId == deviceId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        messages.Should().HaveCount(2);
        messages[0].Content.Should().Be("Hello");
        messages[0].IsUser.Should().BeTrue();
        messages[1].Content.Should().Be("Hi there!");
        messages[1].IsUser.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessageAsync_WithDeviceIdAndStoredKey_UsesStoredKey()
    {
        // Arrange
        var deviceId = "test-device-key";
        var storedKey = "sk-ant-test-key-123";

        _dbContext.ApiKeys.Add(new ApiKeyEntity
        {
            DeviceId = deviceId,
            ApiKey = storedKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new ChatRequestDto("Hello", DeviceId: deviceId);
        _mockAiService.SendMessageAsync("Hello", storedKey, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns("Response with user key");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("Response with user key");
        await _mockAiService.Received(1).SendMessageAsync("Hello", storedKey, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetChatHistoryAsync_ReturnsMessagesForDevice()
    {
        // Arrange
        var deviceId = "history-device";
        _dbContext.ChatMessages.AddRange(
            new ChatMessageEntity { DeviceId = deviceId, Content = "Hello", IsUser = true, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new ChatMessageEntity { DeviceId = deviceId, Content = "Hi!", IsUser = false, Timestamp = DateTime.UtcNow.AddMinutes(-1) },
            new ChatMessageEntity { DeviceId = "other-device", Content = "Other", IsUser = true, Timestamp = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var history = await _sut.GetChatHistoryAsync(deviceId);

        // Assert
        history.Should().HaveCount(2);
        history[0].Content.Should().Be("Hello");
        history[0].IsUser.Should().BeTrue();
        history[1].Content.Should().Be("Hi!");
        history[1].IsUser.Should().BeFalse();
    }

    [Fact]
    public async Task ClearChatHistoryAsync_RemovesOnlyDeviceMessages()
    {
        // Arrange
        var deviceId = "clear-device";
        _dbContext.ChatMessages.AddRange(
            new ChatMessageEntity { DeviceId = deviceId, Content = "Hello", IsUser = true },
            new ChatMessageEntity { DeviceId = deviceId, Content = "Hi!", IsUser = false },
            new ChatMessageEntity { DeviceId = "keep-device", Content = "Keep me", IsUser = true }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.ClearChatHistoryAsync(deviceId);

        // Assert
        var remaining = await _dbContext.ChatMessages.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].DeviceId.Should().Be("keep-device");
    }

    [Fact]
    public async Task ListModelsAsync_WithNoDeviceId_ReturnsModels()
    {
        // Arrange
        var models = new List<ModelInfoDto>
        {
            new("claude-sonnet-4-20250514", "Claude Sonnet 4", DateTime.UtcNow),
            new("claude-haiku-4-20250414", "Claude Haiku 4", DateTime.UtcNow)
        };
        _mockAiService.ListModelsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(models);

        // Act
        var result = await _sut.ListModelsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("claude-sonnet-4-20250514");
        result[0].DisplayName.Should().Be("Claude Sonnet 4");
        result[1].Id.Should().Be("claude-haiku-4-20250414");
    }

    [Fact]
    public async Task ListModelsAsync_WithDeviceIdAndStoredKey_UsesStoredKey()
    {
        // Arrange
        var deviceId = "model-list-device";
        var storedKey = "sk-ant-test-key-models";

        _dbContext.ApiKeys.Add(new ApiKeyEntity
        {
            DeviceId = deviceId,
            ApiKey = storedKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var models = new List<ModelInfoDto>
        {
            new("claude-opus-4-20250514", "Claude Opus 4", DateTime.UtcNow)
        };
        _mockAiService.ListModelsAsync(storedKey, Arg.Any<CancellationToken>())
            .Returns(models);

        // Act
        var result = await _sut.ListModelsAsync(deviceId);

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be("claude-opus-4-20250514");
        await _mockAiService.Received(1).ListModelsAsync(storedKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListModelsAsync_WhenAiServiceThrows_PropagatesException()
    {
        // Arrange
        _mockAiService.ListModelsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<List<ModelInfoDto>>(_ => throw new HttpRequestException("API unavailable"));

        // Act
        var act = () => _sut.ListModelsAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("API unavailable");
    }

    [Fact]
    public async Task SendMessageAsync_WithModelSpecified_PassesModelToAiService()
    {
        // Arrange
        var request = new ChatRequestDto("Hello", Model: "claude-haiku-4-20250414");
        _mockAiService.SendMessageAsync("Hello", Arg.Any<string?>(), "claude-haiku-4-20250414", Arg.Any<CancellationToken>())
            .Returns("Fast response");

        // Act
        var result = await _sut.SendMessageAsync(request);

        // Assert
        result.Response.Should().Be("Fast response");
        await _mockAiService.Received(1).SendMessageAsync("Hello", Arg.Any<string?>(), "claude-haiku-4-20250414", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_NoApiKeyAnywhere_ThrowsInvalidOperationException()
    {
        // Arrange — create a ChatService with NO config key at all
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var sut = new ChatService(_mockAiService, _dbContext, emptyConfig, _mockProtector, _mockLogger);

        var request = new ChatRequestDto("Hello");

        // Act & Assert
        var act = () => sut.SendMessageAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No API key available*");
    }

    [Fact]
    public async Task StreamMessageAsync_NoApiKeyAnywhere_ThrowsInvalidOperationException()
    {
        // Arrange — create a ChatService with NO config key at all
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var sut = new ChatService(_mockAiService, _dbContext, emptyConfig, _mockProtector, _mockLogger);

        var request = new ChatRequestDto("Hello");

        // Act & Assert — must consume the async enumerable to trigger the exception
        var act = async () =>
        {
            await foreach (var _ in sut.StreamMessageAsync(request)) { }
        };
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No API key available*");
    }

    private static async IAsyncEnumerable<string> AsyncEnumerable(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }
}
