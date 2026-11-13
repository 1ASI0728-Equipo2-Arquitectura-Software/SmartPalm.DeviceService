namespace SmartPalm.DeviceService.API.Tests.Integration;

internal record RegistryDeviceResponse(
    string IotDeviceMac,
    long OwnerUserId,
    long PlantationId,
    long SectorId,
    bool IsProvisioned,
    bool IsActive);
