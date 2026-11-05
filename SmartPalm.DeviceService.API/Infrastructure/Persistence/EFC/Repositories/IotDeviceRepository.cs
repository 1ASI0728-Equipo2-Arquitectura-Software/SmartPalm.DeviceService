using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Repositories;

namespace SmartPalm.DeviceService.Infrastructure.Persistence;

internal class IotDeviceRepository(DeviceDbContext context) : IIotDeviceRepository
{
    public async Task AddAsync(IotDevice device, CancellationToken cancellationToken) =>
        await context.IotDevices.AddAsync(device, cancellationToken);

    public Task<IotDevice?> FindByMacAsync(string macAddress, CancellationToken cancellationToken)
    {
        var normalized = EdgeGateway.NormalizeMac(macAddress);
        return context.IotDevices.AsNoTracking()
            .SingleOrDefaultAsync(value => value.MacAddress == normalized, cancellationToken);
    }

    public Task<IotDevice?> FindForUpdateAsync(string macAddress, CancellationToken cancellationToken)
    {
        var normalized = EdgeGateway.NormalizeMac(macAddress);
        return context.IotDevices
            .SingleOrDefaultAsync(value => value.MacAddress == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<IotDevice>> ListByGatewayAsync(
        string gatewayMacAddress,
        CancellationToken cancellationToken)
    {
        var normalized = EdgeGateway.NormalizeMac(gatewayMacAddress);
        return await context.IotDevices.AsNoTracking()
            .Where(value => value.GatewayMacAddress == normalized)
            .OrderBy(value => value.MacAddress)
            .ToArrayAsync(cancellationToken);
    }
}
