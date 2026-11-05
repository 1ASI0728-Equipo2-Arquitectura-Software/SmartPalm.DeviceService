using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.DeviceService.Api.Resources;
using SmartPalm.DeviceService.Api.Transform;
using SmartPalm.DeviceService.Application.Contracts;
using SmartPalm.DeviceService.Application.Services;
using SmartPalm.DeviceService.Domain.Model.Queries;

namespace SmartPalm.DeviceService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/edge-gateways")]
public class EdgeGatewaysController(
    DeviceCommandService commands,
    DeviceQueryService queries) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RegisterEdgeDevice(
        EdgeDeviceRegistrationResource resource,
        CancellationToken cancellationToken)
    {
        var registration = await commands.RegisterGatewayAsync(resource.ToCommand(), cancellationToken);
        return Created($"/api/v1/edge-gateways/{resource.EdgeMac}", registration);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectivityStatusResponse>>> GetAllGateways(CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetAllEdgeGatewaysQuery(), cancellationToken));

    [HttpPost("{gatewayMac}/iot-devices")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RegisterIotDevice(
        string gatewayMac,
        IotDeviceRegistrationResource resource,
        CancellationToken cancellationToken)
    {
        var registration = await commands.RegisterIotDeviceAsync(resource.ToCommand(gatewayMac), cancellationToken);
        return Created($"/api/v1/edge-gateways/{gatewayMac}/iot-devices/{resource.IotMac}", registration);
    }

    [HttpGet("{gatewayMac}/devices")]
    public async Task<ActionResult<GatewayDevicesResponse>> GetGatewayDevices(
        string gatewayMac,
        CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetGatewayDevicesQuery(gatewayMac), cancellationToken));

    [HttpGet("{gatewayMac}/registry")]
    public async Task<ActionResult<EdgeRegistryResponse>> GetDeviceRegistry(
        string gatewayMac,
        CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetEdgeRegistryQuery(gatewayMac), cancellationToken));

    [HttpGet("{gatewayMac}/connectivity")]
    [AllowAnonymous]
    public async Task<ActionResult<ConnectivityStatusResponse>> GetGatewayConnectivityStatus(
        string gatewayMac,
        CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetGatewayConnectivityQuery(gatewayMac), cancellationToken));
}
