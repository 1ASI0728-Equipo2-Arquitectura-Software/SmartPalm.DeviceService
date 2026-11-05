using SmartPalm.DeviceService.Application.Contracts;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Model.Queries;
using SmartPalm.DeviceService.Domain.Repositories;

namespace SmartPalm.DeviceService.Application.Services;

public class DeviceQueryService(IEdgeGatewayRepository gateways, IIotDeviceRepository devices)
{
    private static readonly TimeSpan ConnectivityTimeout = TimeSpan.FromMinutes(1);

    public async Task<IReadOnlyList<ConnectivityStatusResponse>> HandleAsync(
        GetAllEdgeGatewaysQuery query,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return (await gateways.ListAsync(cancellationToken)).Select(value => Map(value, now)).ToArray();
    }

    public async Task<ConnectivityStatusResponse> HandleAsync(
        GetGatewayConnectivityQuery query,
        CancellationToken cancellationToken)
    {
        var gateway = await RequiredGatewayAsync(query.GatewayMac, cancellationToken);
        return Map(gateway, DateTime.UtcNow);
    }

    public async Task<GatewayDevicesResponse> HandleAsync(
        GetGatewayDevicesQuery query,
        CancellationToken cancellationToken)
    {
        var gateway = await RequiredGatewayAsync(query.GatewayMac, cancellationToken);
        var registered = await devices.ListByGatewayAsync(gateway.MacAddress, cancellationToken);
        return new GatewayDevicesResponse(
            gateway.MacAddress,
            registered.Select(value => new GatewayDeviceResponse(
                value.MacAddress,
                value.OwnerUserId,
                value.PlantationId,
                value.SectorId,
                value.BootstrapCredentialHash,
                value.IsProvisioned,
                value.IsActive)).ToArray());
    }

    public async Task<EdgeRegistryResponse> HandleAsync(
        GetEdgeRegistryQuery query,
        CancellationToken cancellationToken)
    {
        var registered = await HandleAsync(new GetGatewayDevicesQuery(query.GatewayMac), cancellationToken);
        return new EdgeRegistryResponse(
            registered.GatewayMac,
            registered.Devices.Select(value => new RegistryIotDeviceResponse(
                value.DeviceMac,
                value.OwnerUserId,
                value.PlantationId,
                value.SectorId,
                value.BootstrapCredentialHash,
                value.IsProvisioned,
                value.IsActive)).ToArray());
    }

    private async Task<EdgeGateway> RequiredGatewayAsync(string mac, CancellationToken cancellationToken) =>
        await gateways.FindByMacAsync(EdgeGateway.NormalizeMac(mac), cancellationToken, tracking: false)
        ?? throw new KeyNotFoundException("Edge Device not found.");

    private static ConnectivityStatusResponse Map(EdgeGateway gateway, DateTime now)
    {
        var connected = gateway.IsConnectedAt(now, ConnectivityTimeout);
        return new ConnectivityStatusResponse(gateway.MacAddress, connected, connected ? "connected" : "offline");
    }
}
