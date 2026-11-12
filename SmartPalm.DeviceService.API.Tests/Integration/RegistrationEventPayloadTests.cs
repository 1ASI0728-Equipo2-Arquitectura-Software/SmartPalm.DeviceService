using System.Text.Json;
using SmartPalm.DeviceService.Domain.Events;

namespace SmartPalm.DeviceService.API.Tests.Integration;

public class RegistrationEventPayloadTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void GatewayRegistrationEvent_ContainsOwnerAndActiveState()
    {
        var payload = new EdgeGatewayRegisteredIntegrationEvent(
            "AA:BB",
            9,
            DateTime.UtcNow,
            42,
            "credential-hash");

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload, JsonOptions));
        var json = document.RootElement;

        Assert.Equal(42, json.GetProperty("ownerUserId").GetInt64());
        Assert.True(json.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public void IotRegistrationEvent_ContainsOwnershipRelationshipAssignmentsAndStates()
    {
        var payload = new IotDeviceRegisteredIntegrationEvent(
            "AA:BB",
            "CC:DD",
            27,
            DateTime.UtcNow,
            42,
            88,
            "bootstrap-hash",
            false,
            true);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload, JsonOptions));
        var json = document.RootElement;

        Assert.Equal("AA:BB", json.GetProperty("edgeDeviceMacAddress").GetString());
        Assert.Equal(42, json.GetProperty("ownerUserId").GetInt64());
        Assert.Equal(27, json.GetProperty("plantationId").GetInt64());
        Assert.Equal(88, json.GetProperty("sectorId").GetInt64());
        Assert.False(json.GetProperty("isProvisioned").GetBoolean());
        Assert.True(json.GetProperty("isActive").GetBoolean());
    }
}
