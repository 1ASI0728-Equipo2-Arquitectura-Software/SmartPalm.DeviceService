namespace SmartPalm.DeviceService.Domain.Model.Commands;

public record RegisterEdgeGatewayCommand(string EdgeMac, long OwnerUserId, long MonitoringZoneId);
