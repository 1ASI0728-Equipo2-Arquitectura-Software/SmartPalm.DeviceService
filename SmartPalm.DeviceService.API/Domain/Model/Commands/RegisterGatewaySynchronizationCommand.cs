namespace SmartPalm.DeviceService.Domain.Model.Commands;

public record RegisterGatewaySynchronizationCommand(string GatewayMac, DateTime OccurredAt);
