namespace SmartPalm.DeviceService.Application.Contracts;

public record EdgeRegistryResponse(string EdgeDeviceMac, IReadOnlyList<RegistryIotDeviceResponse> Registry);
