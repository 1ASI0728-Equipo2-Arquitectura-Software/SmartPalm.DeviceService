namespace SmartPalm.DeviceService.Domain.Model.Commands;

public record RegisterGatewayHeartbeatCommand(string GatewayMac, DateTime OccurredAt);
