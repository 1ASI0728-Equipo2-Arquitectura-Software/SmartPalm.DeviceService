namespace SmartPalm.DeviceService.Domain.Model.Commands;

public record RegisterIotDeviceCommand(
    string GatewayMac,
    string IotMac,
    long OwnerUserId,
    long PlantationId,
    long SectorId);
