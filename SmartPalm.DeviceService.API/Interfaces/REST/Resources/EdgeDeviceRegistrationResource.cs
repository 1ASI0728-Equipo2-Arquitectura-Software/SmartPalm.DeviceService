namespace SmartPalm.DeviceService.Api.Resources;

public record EdgeDeviceRegistrationResource(string EdgeMac, long OwnerUserId, long MonitoringZoneId);
