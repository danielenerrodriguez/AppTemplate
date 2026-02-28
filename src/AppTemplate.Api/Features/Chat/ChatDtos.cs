namespace AppTemplate.Api.Features.Chat;

public record ChatRequestDto(
    string Message,
    string? DeviceId = null,
    string? SystemPrompt = null,
    string? Model = null);

public record ChatResponseDto(
    string Response,
    DateTime Timestamp);

public record ChatHistoryMessageDto(
    string Content,
    bool IsUser,
    DateTime Timestamp);

public record ModelListDto(
    string Id,
    string DisplayName);
