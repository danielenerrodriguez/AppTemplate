namespace AppTemplate.Api.Shared.Data;

/// <summary>
/// Stores a per-device Anthropic API key. Devices are identified by a browser-generated UUID.
/// </summary>
public class ApiKeyEntity
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
