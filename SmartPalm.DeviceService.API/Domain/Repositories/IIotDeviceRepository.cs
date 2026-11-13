using SmartPalm.DeviceService.Domain.Devices;

namespace SmartPalm.DeviceService.Domain.Repositories;

public interface IIotDeviceRepository
{
    Task AddAsync(IotDevice device, CancellationToken cancellationToken);
    Task<IotDevice?> FindByMacAsync(string macAddress, CancellationToken cancellationToken);
    Task<IotDevice?> FindForUpdateAsync(string macAddress, CancellationToken cancellationToken);
    Task<IReadOnlyList<IotDevice>> ListByGatewayAsync(string gatewayMacAddress, CancellationToken cancellationToken);
    void RemoveRange(IEnumerable<IotDevice> devices);
}
