using SmartPalm.DeviceService.Api.Resources;
using SmartPalm.DeviceService.Domain.Model.Commands;

namespace SmartPalm.DeviceService.Api.Transform;

public static class DeviceResourceAssemblers
{
    public static RegisterEdgeGatewayCommand ToCommand(this EdgeDeviceRegistrationResource resource) =>
        new(resource.EdgeMac, resource.OwnerUserId, resource.MonitoringZoneId);

    public static RegisterIotDeviceCommand ToCommand(
        this IotDeviceRegistrationResource resource,
        string gatewayMac) => new(
            gatewayMac,
            resource.IotMac,
            resource.OwnerUserId,
            resource.PlantationId,
            resource.SectorId);
}
