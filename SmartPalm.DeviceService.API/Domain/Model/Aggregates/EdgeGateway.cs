namespace SmartPalm.DeviceService.Domain.Devices;

public class EdgeGateway
{
    private EdgeGateway() { }

    public EdgeGateway(
        string macAddress,
        long ownerUserId,
        long monitoringZoneId,
        string credentialHash,
        DateTime? createdAt = null)
    {
        if (ownerUserId <= 0) throw new ArgumentOutOfRangeException(nameof(ownerUserId));
        if (monitoringZoneId <= 0) throw new ArgumentOutOfRangeException(nameof(monitoringZoneId));
        if (string.IsNullOrWhiteSpace(credentialHash)) throw new ArgumentException("Credential hash is required.", nameof(credentialHash));
        MacAddress = NormalizeMac(macAddress);
        OwnerUserId = ownerUserId;
        MonitoringZoneId = monitoringZoneId;
        CredentialHash = credentialHash;
        IsActive = true;
        CreatedAt = NormalizeUtc(createdAt ?? DateTime.UtcNow);
        LastConnectivityCheckAt = CreatedAt;
    }

    // Compatibility overload for existing domain tests and previously persisted fixtures.
    public EdgeGateway(string macAddress, long monitoringZoneId, DateTime? createdAt = null)
    {
        MacAddress = NormalizeMac(macAddress);
        MonitoringZoneId = monitoringZoneId;
        CreatedAt = NormalizeUtc(createdAt ?? DateTime.UtcNow);
        LastConnectivityCheckAt = CreatedAt;
        IsActive = true;
    }

    public long Id { get; private set; }
    public string MacAddress { get; private set; } = string.Empty;
    public long OwnerUserId { get; private set; }
    public long MonitoringZoneId { get; private set; }
    public string CredentialHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime LastConnectivityCheckAt { get; private set; }
    public DateTime? LastSyncAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsConnectedAt(DateTime utcNow, TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return LastConnectivityCheckAt >= NormalizeUtc(utcNow).Subtract(timeout);
    }

    public void RegisterHeartbeat(DateTime occurredAt) => LastConnectivityCheckAt = NormalizeUtc(occurredAt);

    public void RegisterSynchronization(DateTime occurredAt)
    {
        RegisterHeartbeat(occurredAt);
        LastSyncAt = LastConnectivityCheckAt;
    }

    public void Revoke() => IsActive = false;

    public static string NormalizeMac(string value) =>
        global::SmartPalm.DeviceService.Domain.Devices.MacAddress.Create(value).Value;

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
