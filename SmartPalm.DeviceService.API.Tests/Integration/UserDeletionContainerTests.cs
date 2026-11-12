using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Application.Abstractions;
using SmartPalm.DeviceService.Application.EventHandlers;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Events;
using SmartPalm.DeviceService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SmartPalm.DeviceService.API.Tests.Integration;

public class UserDeletionContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("smartpalm_device")
        .WithUsername("smartpalm_device")
        .WithPassword("device-tests")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task HandleAsync_RemovesDevicesAndGatewaySoMacsCanBeRegisteredAgain()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var gateway = new EdgeGateway("AA:BB:CC:DD:EE:42", 42, 9, "gateway-hash");
        var device = new IotDevice("AA:BB:CC:DD:EE:42", "11:22:33:44:55:66", 42, 27, 88, "iot-hash");
        context.EdgeGateways.Add(gateway);
        context.IotDevices.Add(device);
        await context.SaveChangesAsync();

        var events = new OutboxWriter();
        var handler = new UserDeletionEventHandler(context, events);
        await handler.HandleAsync(
            new UserDeletionRequestedIntegrationEvent(Guid.NewGuid(), 42, DateTime.UtcNow),
            CancellationToken.None);

        Assert.Empty(await context.EdgeGateways.ToListAsync());
        Assert.Empty(await context.IotDevices.ToListAsync());
        Assert.Single(await context.InboxMessages.ToListAsync());
        Assert.Equal("smartpalm.identity.user-deletion-service-completed.v1", events.EventType);

        context.EdgeGateways.Add(new EdgeGateway("AA:BB:CC:DD:EE:42", 84, 10, "new-gateway-hash"));
        await context.SaveChangesAsync();

        Assert.Equal(84, await context.EdgeGateways
            .Where(value => value.MacAddress == "AA:BB:CC:DD:EE:42")
            .Select(value => value.OwnerUserId)
            .SingleAsync());
    }

    private DeviceDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<DeviceDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options);

    private sealed class OutboxWriter : IIntegrationEventWriter
    {
        public string? EventType { get; private set; }

        public void Enqueue<T>(string eventType, T payload) where T : notnull
        {
            EventType = eventType;
        }
    }
}
