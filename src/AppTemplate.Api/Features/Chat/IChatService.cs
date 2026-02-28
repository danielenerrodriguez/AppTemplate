namespace AppTemplate.Api.Features.Chat;

/// <summary>
/// Orchestrates AI chat operations: sending messages, streaming responses,
/// managing per-device chat history, and listing available models.
/// Resolves API keys automatically (per-user key from DB, then env var / config fallback).
/// </summary>
public interface IChatService
{
    /// <summary>Sends a message and returns the full AI response.</summary>
    Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Sends a message and streams the AI response as chunks.</summary>
    IAsyncEnumerable<string> StreamMessageAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Returns the stored chat history for a device.</summary>
    Task<List<ChatHistoryMessageDto>> GetChatHistoryAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Deletes all chat history for a device.</summary>
    Task ClearChatHistoryAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>Lists available AI models. Uses the device's stored API key if available.</summary>
    Task<List<ModelListDto>> ListModelsAsync(string? deviceId = null, CancellationToken cancellationToken = default);
}
