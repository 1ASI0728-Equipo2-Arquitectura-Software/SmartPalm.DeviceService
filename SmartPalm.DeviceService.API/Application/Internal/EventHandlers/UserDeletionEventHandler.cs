using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Application.Abstractions;
using SmartPalm.DeviceService.Domain.Events;
using SmartPalm.DeviceService.Infrastructure.Messaging;
using SmartPalm.DeviceService.Infrastructure.Persistence;

namespace SmartPalm.DeviceService.Application.EventHandlers;

public class UserDeletionEventHandler(DeviceDbContext db, IIntegrationEventWriter events)
{
    public async Task HandleAsync(UserDeletionRequestedIntegrationEvent message, CancellationToken token)
    {
        if (await db.InboxMessages.AnyAsync(x => x.Id == message.OperationId, token)) return;

        var devices = await db.IotDevices.Where(x => x.OwnerUserId == message.UserId).ToArrayAsync(token);
        var gateways = await db.EdgeGateways.Where(x => x.OwnerUserId == message.UserId).ToArrayAsync(token);

        db.IotDevices.RemoveRange(devices);
        db.EdgeGateways.RemoveRange(gateways);
        db.InboxMessages.Add(new InboxMessage(message.OperationId, "smartpalm.identity.user-deletion-requested.v1"));
        events.Enqueue("smartpalm.identity.user-deletion-service-completed.v1",
            new UserDeletionServiceCompletedIntegrationEvent(
                message.OperationId, message.UserId, "device", devices.Length + gateways.Length, DateTime.UtcNow));

        await db.SaveChangesAsync(token);
    }
}
