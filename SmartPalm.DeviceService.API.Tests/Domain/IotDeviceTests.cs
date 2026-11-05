using SmartPalm.DeviceService.Domain.Devices;

namespace SmartPalm.DeviceService.API.Tests.Domain;

public class IotDeviceTests
{
    [Fact]
    public void Constructor_NormalizesMacsAndPreservesExternalPlantationId()
    {
        var device = new IotDevice(" aa:bb ", " cc:dd ", 42);

        Assert.Equal("AA:BB", device.GatewayMacAddress);
        Assert.Equal("CC:DD", device.MacAddress);
        Assert.Equal(42, device.PlantationId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsInvalidPlantationId(long plantationId) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new IotDevice("AA:BB", "CC:DD", plantationId));
}
