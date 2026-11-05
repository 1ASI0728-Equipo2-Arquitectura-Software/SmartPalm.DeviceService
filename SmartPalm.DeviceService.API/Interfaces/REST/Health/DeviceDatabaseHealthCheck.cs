using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartPalm.DeviceService.Infrastructure.Persistence;

namespace SmartPalm.DeviceService.Api;

public class DeviceDatabaseHealthCheck(DeviceDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthContext,
        CancellationToken cancellationToken = default) =>
        await context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Device database is unavailable.");
}
