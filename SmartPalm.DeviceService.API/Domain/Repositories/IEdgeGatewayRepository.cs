using SmartPalm.DeviceService.Domain.Devices;

namespace SmartPalm.DeviceService.Domain.Repositories;

public interface IEdgeGatewayRepository
{
    Task AddAsync(EdgeGateway gateway, CancellationToken cancellationToken);
    Task<EdgeGateway?> FindByMacAsync(string macAddress, CancellationToken cancellationToken, bool tracking = true);
    Task<IReadOnlyList<EdgeGateway>> ListAsync(CancellationToken cancellationToken);
    void Remove(EdgeGateway gateway);
}
