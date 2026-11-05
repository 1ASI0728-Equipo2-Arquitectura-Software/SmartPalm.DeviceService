namespace SmartPalm.DeviceService.Application.Contracts;

public record GatewayDevicesResponse(string GatewayMac, IReadOnlyList<GatewayDeviceResponse> Devices);
