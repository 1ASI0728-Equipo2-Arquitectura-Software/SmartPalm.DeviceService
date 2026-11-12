namespace SmartPalm.DeviceService.Domain.Events;

public record UserDeletionRequestedIntegrationEvent(Guid OperationId, int UserId, DateTime OccurredAtUtc);
