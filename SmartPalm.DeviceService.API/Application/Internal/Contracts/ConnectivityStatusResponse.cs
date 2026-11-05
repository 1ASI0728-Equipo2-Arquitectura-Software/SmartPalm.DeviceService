namespace SmartPalm.DeviceService.Application.Contracts;

public record ConnectivityStatusResponse(string Mac, bool IsConnected, string Status);
