namespace AppTemplate.Api.Shared.AI;

/// <summary>
/// Low-level AI service abstraction. Wraps the Anthropic SDK and supports
/// both a default client (using ANTHROPIC_API_KEY env var) and per-request
/// API key overrides for multi-user scenarios.
/// </summary>
public interface IAIService
{
    /// <summary>Sends a prompt using the default API key and model.</summary>
    Task<string> SendMessageAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>Sends a prompt using a specific API key and the default model.</summary>
    Task<string> SendMessageAsync(string prompt, string apiKey, CancellationToken cancellationToken = default);

    /// <summary>Sends a prompt with optional per-request API key and model override.</summary>
    Task<string> SendMessageAsync(string prompt, string? apiKey, string? model, CancellationToken cancellationToken = default);

    /// <summary>Streams a response using the default API key and model.</summary>
    IAsyncEnumerable<string> StreamMessageAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>Streams a response using a specific API key and the default model.</summary>
    IAsyncEnumerable<string> StreamMessageAsync(string prompt, string apiKey, CancellationToken cancellationToken = default);

    /// <summary>Streams a response with optional per-request API key and model override.</summary>
    IAsyncEnumerable<string> StreamMessageAsync(string prompt, string? apiKey, string? model, CancellationToken cancellationToken = default);

    /// <summary>Lists available models from the AI provider.</summary>
    Task<List<ModelInfoDto>> ListModelsAsync(string? apiKey = null, CancellationToken cancellationToken = default);
}

/// <summary>Describes an available AI model.</summary>
public record ModelInfoDto(string Id, string DisplayName, DateTime CreatedAt);
