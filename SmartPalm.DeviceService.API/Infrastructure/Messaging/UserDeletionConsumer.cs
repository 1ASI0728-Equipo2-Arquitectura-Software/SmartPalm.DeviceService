using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartPalm.DeviceService.Application.EventHandlers;
using SmartPalm.DeviceService.Domain.Events;
using SmartPalm.DeviceService.Infrastructure.Configuration;

namespace SmartPalm.DeviceService.Infrastructure.Messaging;

internal class UserDeletionConsumer(IServiceScopeFactory scopes, IOptions<RabbitMqOptions> options, ILogger<UserDeletionConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions settings = options.Value;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!settings.Enabled) return;
        while (!token.IsCancellationRequested)
        {
            try { await ConsumeAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception exception) { logger.LogWarning(exception, "Device deletion consumer disconnected."); await Task.Delay(3000, token); }
        }
    }

    private async Task ConsumeAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory { Uri = new Uri(settings.Uri), AutomaticRecoveryEnabled = true };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);
        await channel.ExchangeDeclareAsync(settings.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        const string queue = "smartpalm.device.user-deletion";
        await channel.QueueDeclareAsync(queue, true, false, false, cancellationToken: token);
        await channel.QueueBindAsync(queue, settings.Exchange, "smartpalm.identity.user-deletion-requested.v1", cancellationToken: token);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                using var document = JsonDocument.Parse(args.Body.ToArray());
                var payload = document.RootElement.TryGetProperty("payload", out var value) ? value : document.RootElement;
                var message = payload.Deserialize<UserDeletionRequestedIntegrationEvent>(new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException("Invalid deletion event.");
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<UserDeletionEventHandler>().HandleAsync(message, token);
                await channel.BasicAckAsync(args.DeliveryTag, false, token);
            }
            catch (Exception exception) { logger.LogError(exception, "Device deletion event failed."); await channel.BasicNackAsync(args.DeliveryTag, false, true, token); }
        };
        await channel.BasicConsumeAsync(queue, false, consumer, token);
        await Task.Delay(Timeout.InfiniteTimeSpan, token);
    }
}
