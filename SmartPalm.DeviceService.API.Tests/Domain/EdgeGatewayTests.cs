using SmartPalm.DeviceService.Domain.Devices;

namespace SmartPalm.DeviceService.API.Tests.Domain;

public class EdgeGatewayTests
{
    [Fact]
    public void Constructor_NormalizesMacAndStartsConnected()
    {
        var now = DateTime.UtcNow;
        var gateway = new EdgeGateway(" aa:bb:cc:dd:ee:01 ", 7, now);

        Assert.Equal("AA:BB:CC:DD:EE:01", gateway.MacAddress);
        Assert.Equal(7, gateway.MonitoringZoneId);
        Assert.True(gateway.IsConnectedAt(now.AddSeconds(59), TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void RegisterSynchronization_UpdatesHeartbeatAndSynchronizationTime()
    {
        var gateway = new EdgeGateway("AA:BB", 1, DateTime.UtcNow.AddMinutes(-5));
        var synchronizedAt = DateTime.UtcNow;

        gateway.RegisterSynchronization(synchronizedAt);

        Assert.Equal(synchronizedAt, gateway.LastConnectivityCheckAt);
        Assert.Equal(synchronizedAt, gateway.LastSyncAt);
    }

    [Fact]
    public void IsConnectedAt_ReturnsFalseAfterTimeout()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-2);
        var gateway = new EdgeGateway("AA:BB", 1, createdAt);

        Assert.False(gateway.IsConnectedAt(DateTime.UtcNow, TimeSpan.FromMinutes(1)));
    }
}
