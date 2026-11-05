using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.DeviceService.Application.Services;

namespace SmartPalm.DeviceService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/v1/admin/iot-devices")]
public class AdminIotDevicesController(DeviceCommandService commands) : ControllerBase
{
    [HttpPost("{iotMac}/provision")]
    public async Task<IActionResult> MarkProvisioned(string iotMac, CancellationToken cancellationToken)
    {
        await commands.MarkIotProvisionedAsync(iotMac, cancellationToken);
        return Ok(new { iotMac, isProvisioned = true });
    }

    [HttpPost("{iotMac}/revoke")]
    public async Task<IActionResult> RevokeProvisioning(string iotMac, CancellationToken cancellationToken)
    {
        await commands.RevokeIotProvisioningAsync(iotMac, cancellationToken);
        return Ok(new { iotMac, isProvisioned = false });
    }
}
