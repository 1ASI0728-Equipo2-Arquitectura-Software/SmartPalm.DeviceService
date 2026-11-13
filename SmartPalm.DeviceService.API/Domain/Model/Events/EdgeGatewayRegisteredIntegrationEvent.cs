namespace SmartPalm.DeviceService.Domain.Events;

public record EdgeGatewayRegisteredIntegrationEvent(
    string EdgeMac,
    long MonitoringZoneId,
    DateTime CreatedAt,
    long OwnerUserId = 0,
    string CredentialHash = "",
    bool IsActive = true);
