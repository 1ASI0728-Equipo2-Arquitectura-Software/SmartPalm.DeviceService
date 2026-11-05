namespace SmartPalm.DeviceService.Application.Contracts;

public record IotRegistrationResponse(
    string IotMac,
    long OwnerUserId,
    long PlantationId,
    long SectorId,
    string BootstrapKey,
    DateTime CreatedAt);
