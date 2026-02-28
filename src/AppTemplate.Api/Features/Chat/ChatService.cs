using AppTemplate.Api.Shared.AI;
using AppTemplate.Api.Shared.Auth;
using AppTemplate.Api.Shared.Data;
using AppTemplate.Api.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Api.Features.Chat;

public class ChatService : IChatService
{
    private readonly IAIService _aiService;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IApiKeyProtector _protector;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IAIService aiService,
        AppDbContext dbContext,
        IConfiguration configuration,
        IApiKeyProtector protector,
        ILogger<ChatService> logger)
    {
        _aiService = aiService;
        _dbContext = dbContext;
        _configuration = configuration;
        _protector = protector;
        _logger = logger;
    }

    public async Task<ChatResponseDto> SendMessageAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing chat message for device {DeviceId}", request.DeviceId ?? "anonymous");

        var prompt = BuildPrompt(request);
        var resolved = await ResolveApiKeyAsync(request.DeviceId, cancellationToken);

        if (!resolved.HasKey)
        {
            throw new InvalidOperationException("No API key available. Please configure an API key.");
        }

        var response = await _aiService.SendMessageAsync(prompt, resolved.ApiKey, request.Model, cancellationToken);

        // Persist messages if device ID is provided
        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            await SaveMessagesAsync(request.DeviceId, request.Message, response, cancellationToken);
        }

        return new ChatResponseDto(
            Response: response,
            Timestamp: DateTime.UtcNow);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        ChatRequestDto request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing streaming chat message for device {DeviceId}", request.DeviceId ?? "anonymous");

        var prompt = BuildPrompt(request);
        var resolved = await ResolveApiKeyAsync(request.DeviceId, cancellationToken);

        if (!resolved.HasKey)
        {
            throw new InvalidOperationException("No API key available. Please configure an API key.");
        }

        var stream = _aiService.StreamMessageAsync(prompt, resolved.ApiKey, request.Model, cancellationToken);

        var fullResponse = new System.Text.StringBuilder();
        await foreach (var chunk in stream)
        {
            fullResponse.Append(chunk);
            yield return chunk;
        }

        // Persist messages if device ID is provided
        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            await SaveMessagesAsync(request.DeviceId, request.Message, fullResponse.ToString(), cancellationToken);
        }
    }

    public async Task<List<ChatHistoryMessageDto>> GetChatHistoryAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ChatMessages
            .Where(m => m.DeviceId == deviceId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new ChatHistoryMessageDto(m.Content, m.IsUser, m.Timestamp))
            .ToListAsync(cancellationToken);
    }

    public async Task ClearChatHistoryAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.ChatMessages
            .Where(m => m.DeviceId == deviceId)
            .ToListAsync(cancellationToken);

        _dbContext.ChatMessages.RemoveRange(messages);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ModelListDto>> ListModelsAsync(
        string? deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveApiKeyAsync(deviceId, cancellationToken);
        var models = await _aiService.ListModelsAsync(resolved.ApiKey, cancellationToken);

        return models
            .Select(m => new ModelListDto(m.Id, m.DisplayName))
            .ToList();
    }

    /// <summary>
    /// Resolves the API key to use for a request. Returns a discriminated result
    /// so callers can distinguish "use default client" (null key) from "no key at all".
    /// </summary>
    private async Task<ResolvedApiKey> ResolveApiKeyAsync(string? deviceId, CancellationToken cancellationToken)
    {
        // Priority 1: Per-user key from database
        if (!string.IsNullOrEmpty(deviceId))
        {
            var keyEntity = await _dbContext.ApiKeys
                .FirstOrDefaultAsync(k => k.DeviceId == deviceId, cancellationToken);

            if (keyEntity is not null)
            {
                _logger.LogInformation("Using per-user API key for device {DeviceId}", deviceId);
                var decryptedKey = _protector.Unprotect(keyEntity.ApiKey);
                return new ResolvedApiKey(HasKey: true, ApiKey: decryptedKey);
            }
        }

        // Priority 2: Environment variable or config (null key = use default AnthropicClient)
        var envKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            ?? _configuration.GetValue<string>("Anthropic:ApiKey");

        if (!string.IsNullOrEmpty(envKey))
        {
            _logger.LogInformation("Using environment API key");
            return new ResolvedApiKey(HasKey: true, ApiKey: null);
        }

        // No key available anywhere
        _logger.LogWarning("No API key available for device {DeviceId}", deviceId);
        return new ResolvedApiKey(HasKey: false, ApiKey: null);
    }

    private record ResolvedApiKey(bool HasKey, string? ApiKey);

    private async Task SaveMessagesAsync(
        string deviceId,
        string userMessage,
        string aiResponse,
        CancellationToken cancellationToken)
    {
        _dbContext.ChatMessages.Add(new ChatMessageEntity
        {
            DeviceId = deviceId,
            Content = userMessage,
            IsUser = true,
            Timestamp = DateTime.UtcNow
        });

        _dbContext.ChatMessages.Add(new ChatMessageEntity
        {
            DeviceId = deviceId,
            Content = aiResponse,
            IsUser = false,
            Timestamp = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildPrompt(ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            return request.Message;
        }

        return $"System: {request.SystemPrompt}\n\nUser: {request.Message}";
    }
}
