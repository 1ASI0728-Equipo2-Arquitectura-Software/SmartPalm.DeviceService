namespace SmartPalm.DeviceService.API.Tests.Integration;

internal record RegistryResponse(string EdgeDeviceMac, IReadOnlyList<RegistryDeviceResponse> Registry);
