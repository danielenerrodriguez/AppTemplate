namespace AppTemplate.Api.Shared.Data;

/// <summary>
/// Stores a single chat message (user or AI) associated with a device.
/// </summary>
public class ChatMessageEntity
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
