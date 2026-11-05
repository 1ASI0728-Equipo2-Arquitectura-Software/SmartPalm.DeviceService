namespace SmartPalm.DeviceService.Application.Contracts;

public record GatewayDeviceResponse(
    string DeviceMac,
    long OwnerUserId = 0,
    long PlantationId = 0,
    long SectorId = 0,
    string BootstrapCredentialHash = "",
    bool IsProvisioned = false,
    bool IsActive = true);
