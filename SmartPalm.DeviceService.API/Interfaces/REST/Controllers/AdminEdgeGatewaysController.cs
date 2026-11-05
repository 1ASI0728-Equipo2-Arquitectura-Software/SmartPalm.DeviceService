using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.DeviceService.Api.Resources;
using SmartPalm.DeviceService.Api.Transform;
using SmartPalm.DeviceService.Application.Contracts;
using SmartPalm.DeviceService.Application.Services;
using SmartPalm.DeviceService.Domain.Model.Queries;

namespace SmartPalm.DeviceService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/v1/admin/edge-gateways")]
public class AdminEdgeGatewaysController(
    DeviceCommandService commands,
    DeviceQueryService queries) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterEdgeGateway(
        EdgeDeviceRegistrationResource resource,
        CancellationToken cancellationToken)
    {
        var registration = await commands.RegisterGatewayAsync(resource.ToCommand(), cancellationToken);
        return Created($"/api/v1/admin/edge-gateways/{resource.EdgeMac}", registration);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectivityStatusResponse>>> ListEdgeGateways(CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetAllEdgeGatewaysQuery(), cancellationToken));

    [HttpGet("{gatewayMac}/devices")]
    public async Task<ActionResult<GatewayDevicesResponse>> ListGatewayDevices(
        string gatewayMac,
        CancellationToken cancellationToken) =>
        Ok(await queries.HandleAsync(new GetGatewayDevicesQuery(gatewayMac), cancellationToken));

    [HttpPost("{gatewayMac}/iot-devices")]
    public async Task<IActionResult> RegisterIotDevice(
        string gatewayMac,
        IotDeviceRegistrationResource resource,
        CancellationToken cancellationToken)
    {
        var registration = await commands.RegisterIotDeviceAsync(resource.ToCommand(gatewayMac), cancellationToken);
        return Created($"/api/v1/admin/edge-gateways/{gatewayMac}/iot-devices/{resource.IotMac}", registration);
    }
}
