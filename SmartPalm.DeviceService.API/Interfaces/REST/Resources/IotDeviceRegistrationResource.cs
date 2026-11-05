namespace SmartPalm.DeviceService.Api.Resources;

public record IotDeviceRegistrationResource(string IotMac, long OwnerUserId, long PlantationId, long SectorId);
