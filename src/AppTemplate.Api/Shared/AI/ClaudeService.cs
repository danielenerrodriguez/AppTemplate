using System.Runtime.CompilerServices;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

namespace AppTemplate.Api.Shared.AI;

public class ClaudeService : IAIService
{
    private readonly AnthropicClient _client;
    private readonly string _defaultModel;
    private readonly int _maxTokens;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _client = new AnthropicClient();
        _defaultModel = configuration.GetValue<string>("Anthropic:Model") ?? AnthropicModels.Claude4Sonnet;
        _maxTokens = configuration.GetValue<int?>("Anthropic:MaxTokens") ?? 1024;
        _logger = logger;
    }

    public Task<string> SendMessageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return SendMessageCoreAsync(prompt, _client, _defaultModel, cancellationToken);
    }

    public Task<string> SendMessageAsync(string prompt, string apiKey, CancellationToken cancellationToken = default)
    {
        var client = new AnthropicClient(apiKey);
        return SendMessageCoreAsync(prompt, client, _defaultModel, cancellationToken);
    }

    public Task<string> SendMessageAsync(string prompt, string? apiKey, string? model, CancellationToken cancellationToken = default)
    {
        var client = apiKey is not null ? new AnthropicClient(apiKey) : _client;
        var effectiveModel = model ?? _defaultModel;
        return SendMessageCoreAsync(prompt, client, effectiveModel, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamMessageAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return StreamMessageCoreAsync(prompt, _client, _defaultModel, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamMessageAsync(string prompt, string apiKey, CancellationToken cancellationToken = default)
    {
        var client = new AnthropicClient(apiKey);
        return StreamMessageCoreAsync(prompt, client, _defaultModel, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamMessageAsync(string prompt, string? apiKey, string? model, CancellationToken cancellationToken = default)
    {
        var client = apiKey is not null ? new AnthropicClient(apiKey) : _client;
        var effectiveModel = model ?? _defaultModel;
        return StreamMessageCoreAsync(prompt, client, effectiveModel, cancellationToken);
    }

    public async Task<List<ModelInfoDto>> ListModelsAsync(string? apiKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = apiKey is not null ? new AnthropicClient(apiKey) : _client;
            var result = await client.Models.ListModelsAsync(limit: 100, ctx: cancellationToken);

            return result.Models
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new ModelInfoDto(m.Id, m.DisplayName, m.CreatedAt))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Claude models");
            throw;
        }
    }

    private async Task<string> SendMessageCoreAsync(
        string prompt,
        AnthropicClient client,
        string model,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending message to Claude model {Model}", model);

            var messages = new List<Message>
            {
                new(RoleType.User, prompt)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                MaxTokens = _maxTokens,
                Model = model,
                Stream = false
            };

            var result = await client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            _logger.LogInformation("Received response from Claude");
            return result.Message.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Claude");
            throw;
        }
    }

    private async IAsyncEnumerable<string> StreamMessageCoreAsync(
        string prompt,
        AnthropicClient client,
        string model,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting streaming message to Claude model {Model}", model);

        var messages = new List<Message>
        {
            new(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
                MaxTokens = _maxTokens,
                Model = model,
                Stream = true
        };

        // Yield chunks as they arrive — real streaming, not buffered
        await foreach (var res in client.Messages.StreamClaudeMessageAsync(parameters, cancellationToken))
        {
            if (res.Delta?.Text is not null)
            {
                yield return res.Delta.Text;
            }
        }

        _logger.LogInformation("Completed streaming response from Claude");
    }
}
