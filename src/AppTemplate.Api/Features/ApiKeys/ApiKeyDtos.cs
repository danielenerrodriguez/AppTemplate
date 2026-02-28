namespace AppTemplate.Api.Features.ApiKeys;

public record ApiKeyRequestDto(
    string DeviceId,
    string ApiKey);

public record ApiKeyStatusDto(
    bool HasKey,
    string? MaskedKey = null);
