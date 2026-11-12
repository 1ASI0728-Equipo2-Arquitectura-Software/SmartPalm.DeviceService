using System.Security.Cryptography;
using System.Text;
using SmartPalm.DeviceService.Application.Abstractions;
using SmartPalm.DeviceService.Application.Contracts;
using SmartPalm.DeviceService.Domain.Model.Commands;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Events;
using SmartPalm.DeviceService.Domain.Repositories;

namespace SmartPalm.DeviceService.Application.Services;

public class DeviceCommandService(
    IEdgeGatewayRepository gateways,
    IIotDeviceRepository devices,
    IUnitOfWork unitOfWork,
    IIntegrationEventWriter events)
{
    public async Task<GatewayRegistrationResponse> RegisterGatewayAsync(
        RegisterEdgeGatewayCommand command,
        CancellationToken cancellationToken)
    {
        if (command.OwnerUserId <= 0) throw new ArgumentOutOfRangeException(nameof(command.OwnerUserId));
        var mac = EdgeGateway.NormalizeMac(command.EdgeMac);
        if (await gateways.FindByMacAsync(mac, cancellationToken, tracking: false) is not null)
            throw new InvalidOperationException("Edge Device already registered.");

        var rawKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
        GatewayRegistrationResponse? response = null;
        await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var gateway = new EdgeGateway(mac, command.OwnerUserId, command.MonitoringZoneId, keyHash);
            await gateways.AddAsync(gateway, transactionToken);
            events.Enqueue("device.edge-gateway-registered.v1", new EdgeGatewayRegisteredIntegrationEvent(
                gateway.MacAddress,
                gateway.MonitoringZoneId,
                gateway.CreatedAt,
                gateway.OwnerUserId,
                gateway.CredentialHash));
            await unitOfWork.SaveChangesAsync(transactionToken);
            response = new GatewayRegistrationResponse(
                gateway.MacAddress,
                gateway.OwnerUserId,
                rawKey,
                gateway.CreatedAt);
            return true;
        }, cancellationToken);
        return response!;
    }

    public async Task<IotRegistrationResponse> RegisterIotDeviceAsync(
        RegisterIotDeviceCommand command,
        CancellationToken cancellationToken)
    {
        var gatewayMac = EdgeGateway.NormalizeMac(command.GatewayMac);
        var deviceMac = EdgeGateway.NormalizeMac(command.IotMac);
        if (await gateways.FindByMacAsync(gatewayMac, cancellationToken, tracking: false) is null)
            throw new KeyNotFoundException("Edge Device not found.");
        if (await devices.FindByMacAsync(deviceMac, cancellationToken) is not null)
            throw new InvalidOperationException("IoT Device already registered.");

        var rawBootstrapKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var bootstrapHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBootstrapKey)));
        IotRegistrationResponse? response = null;
        await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var gateway = await gateways.FindByMacAsync(gatewayMac, transactionToken, tracking: true)
                ?? throw new KeyNotFoundException("Edge Device not found.");
            if (!gateway.IsActive) throw new InvalidOperationException("Edge Device is inactive.");
            if (gateway.OwnerUserId != command.OwnerUserId)
                throw new UnauthorizedAccessException("Gateway and IoT device must belong to the same user.");
            var device = new IotDevice(
                gatewayMac,
                deviceMac,
                command.OwnerUserId,
                command.PlantationId,
                command.SectorId,
                bootstrapHash);
            await devices.AddAsync(device, transactionToken);
            events.Enqueue("device.iot-device-registered.v1", new IotDeviceRegisteredIntegrationEvent(
                device.GatewayMacAddress,
                device.MacAddress,
                device.PlantationId,
                device.CreatedAt,
                device.OwnerUserId,
                device.SectorId,
                device.BootstrapCredentialHash,
                device.IsProvisioned));
            await unitOfWork.SaveChangesAsync(transactionToken);
            response = new IotRegistrationResponse(
                device.MacAddress,
                device.OwnerUserId,
                device.PlantationId,
                device.SectorId,
                rawBootstrapKey,
                device.CreatedAt);
            return true;
        }, cancellationToken);
        return response!;
    }

    public async Task RegisterHeartbeatAsync(RegisterGatewayHeartbeatCommand command, CancellationToken cancellationToken)
    {
        var gateway = await RequiredGatewayAsync(command.GatewayMac, cancellationToken);
        gateway.RegisterHeartbeat(command.OccurredAt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkIotProvisionedAsync(string iotMac, CancellationToken cancellationToken)
    {
        var device = await devices.FindForUpdateAsync(iotMac, cancellationToken)
            ?? throw new KeyNotFoundException("IoT Device not found.");
        device.MarkProvisioned();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeIotProvisioningAsync(string iotMac, CancellationToken cancellationToken)
    {
        var device = await devices.FindForUpdateAsync(iotMac, cancellationToken)
            ?? throw new KeyNotFoundException("IoT Device not found.");
        device.ClearProvisioning();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterSynchronizationAsync(
        RegisterGatewaySynchronizationCommand command,
        CancellationToken cancellationToken)
    {
        var gateway = await RequiredGatewayAsync(command.GatewayMac, cancellationToken);
        gateway.RegisterSynchronization(command.OccurredAt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGatewayAsync(string gatewayMac, CancellationToken cancellationToken)
    {
        var normalizedMac = EdgeGateway.NormalizeMac(gatewayMac);

        await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var gateway = await gateways.FindByMacAsync(normalizedMac, transactionToken, tracking: true)
                ?? throw new KeyNotFoundException("Edge Device not found.");
            var associatedDevices = await devices.ListByGatewayAsync(normalizedMac, transactionToken);

            devices.RemoveRange(associatedDevices);
            gateways.Remove(gateway);
            await unitOfWork.SaveChangesAsync(transactionToken);
            return true;
        }, cancellationToken);
    }

    private async Task<EdgeGateway> RequiredGatewayAsync(string mac, CancellationToken cancellationToken) =>
        await gateways.FindByMacAsync(EdgeGateway.NormalizeMac(mac), cancellationToken)
        ?? throw new KeyNotFoundException("Edge Device not found.");
}
