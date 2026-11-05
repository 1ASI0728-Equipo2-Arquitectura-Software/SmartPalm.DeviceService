using System.Text.Json;
using SmartPalm.DeviceService.Application.Abstractions;
using SmartPalm.DeviceService.Infrastructure.Persistence;

namespace SmartPalm.DeviceService.Infrastructure.Messaging;

internal class IntegrationEventWriter(DeviceDbContext context) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue<T>(string eventType, T payload) where T : notnull =>
        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Content = JsonSerializer.Serialize(payload, SerializerOptions),
            OccurredOnUtc = DateTime.UtcNow
        });
}
