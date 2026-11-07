namespace SmartPalm.DeviceService.Infrastructure.Messaging;

public class InboxMessage
{
    private InboxMessage() { }
    public InboxMessage(Guid id, string eventType) { Id = id; EventType = eventType; ProcessedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; private set; }
}
