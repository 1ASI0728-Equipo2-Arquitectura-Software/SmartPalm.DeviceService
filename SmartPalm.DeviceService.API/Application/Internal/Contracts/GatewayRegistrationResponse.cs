namespace SmartPalm.DeviceService.Application.Contracts;

public record GatewayRegistrationResponse(
    string GatewayMac,
    long OwnerUserId,
    string CloudKey,
    DateTime CreatedAt);
