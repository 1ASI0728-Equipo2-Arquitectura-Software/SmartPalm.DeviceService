namespace SmartPalm.DeviceService.Domain.Devices;

public class IotDevice
{
    private IotDevice() { }

    public IotDevice(
        string gatewayMacAddress,
        string macAddress,
        long ownerUserId,
        long plantationId,
        long sectorId,
        string bootstrapCredentialHash = "",
        DateTime? createdAt = null)
    {
        if (ownerUserId <= 0) throw new ArgumentOutOfRangeException(nameof(ownerUserId));
        if (plantationId <= 0) throw new ArgumentOutOfRangeException(nameof(plantationId));
        if (sectorId <= 0) throw new ArgumentOutOfRangeException(nameof(sectorId));
        GatewayMacAddress = EdgeGateway.NormalizeMac(gatewayMacAddress);
        MacAddress = EdgeGateway.NormalizeMac(macAddress);
        OwnerUserId = ownerUserId;
        PlantationId = plantationId;
        SectorId = sectorId;
        BootstrapCredentialHash = bootstrapCredentialHash ?? string.Empty;
        IsActive = true;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    // Compatibility overload for existing domain tests and old data import scripts.
    public IotDevice(string gatewayMacAddress, string macAddress, long plantationId, DateTime? createdAt = null)
    {
        if (plantationId <= 0) throw new ArgumentOutOfRangeException(nameof(plantationId));
        GatewayMacAddress = EdgeGateway.NormalizeMac(gatewayMacAddress);
        MacAddress = EdgeGateway.NormalizeMac(macAddress);
        PlantationId = plantationId;
        IsActive = true;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public string MacAddress { get; private set; } = string.Empty;
    public string GatewayMacAddress { get; private set; } = string.Empty;
    public long OwnerUserId { get; private set; }
    public long PlantationId { get; private set; }
    public long SectorId { get; private set; }
    public string BootstrapCredentialHash { get; private set; } = string.Empty;
    public bool IsProvisioned { get; private set; }
    public DateTime? ProvisionedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void MarkProvisioned(DateTime? provisionedAt = null)
    {
        IsProvisioned = true;
        ProvisionedAt = provisionedAt ?? DateTime.UtcNow;
    }

    public void ClearProvisioning()
    {
        IsProvisioned = false;
        ProvisionedAt = null;
    }

    public void Revoke() => IsActive = false;
}
