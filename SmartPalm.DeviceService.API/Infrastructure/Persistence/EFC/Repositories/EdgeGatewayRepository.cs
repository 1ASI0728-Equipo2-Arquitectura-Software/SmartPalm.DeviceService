using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Repositories;

namespace SmartPalm.DeviceService.Infrastructure.Persistence;

internal class EdgeGatewayRepository(DeviceDbContext context) : IEdgeGatewayRepository
{
    public async Task AddAsync(EdgeGateway gateway, CancellationToken cancellationToken) =>
        await context.EdgeGateways.AddAsync(gateway, cancellationToken);

    public Task<EdgeGateway?> FindByMacAsync(string macAddress, CancellationToken cancellationToken, bool tracking = true)
    {
        var query = tracking ? context.EdgeGateways : context.EdgeGateways.AsNoTracking();
        var normalized = EdgeGateway.NormalizeMac(macAddress);
        return query.SingleOrDefaultAsync(value => value.MacAddress == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<EdgeGateway>> ListAsync(CancellationToken cancellationToken) =>
        await context.EdgeGateways.AsNoTracking().OrderBy(value => value.MacAddress).ToArrayAsync(cancellationToken);
}
