namespace SmartPalm.DeviceService.Domain.Events;

public record EdgeGatewayConnectivityChangedIntegrationEvent(
    string EdgeMac, bool IsConnected, DateTime OccurredAt);
