using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.DeviceService.API.Tests.Integration;

public class DeviceApiContainerTests : IAsyncLifetime
{
    private const string JwtKey = "device-tests-signing-key-with-at-least-32-characters";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("smartpalm_device")
        .WithUsername("smartpalm_device")
        .WithPassword("device-tests")
        .Build();
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();

    public async Task InitializeAsync() =>
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());

    public async Task DisposeAsync() =>
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task RegisterGatewayAndDevice_PersistsStateAndPublishesOutboxMessages()
    {
        using var factory = CreateFactory(rabbitEnabled: true);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());

        var gateway = await client.PostAsJsonAsync("/api/v1/edge-gateways", new
        {
            edgeMac = "aa:bb:cc:dd:ee:01",
            ownerUserId = 42,
            monitoringZoneId = 9
        });
        Assert.Equal(HttpStatusCode.Created, gateway.StatusCode);

        var device = await client.PostAsJsonAsync("/api/v1/edge-gateways/AA:BB:CC:DD:EE:01/iot-devices", new
        {
            iotMac = "11:22:33:44:55:66",
            ownerUserId = 42,
            plantationId = 27,
            sectorId = 88
        });
        Assert.Equal(HttpStatusCode.Created, device.StatusCode);

        var registry = await client.GetFromJsonAsync<RegistryResponse>(
            "/api/v1/edge-gateways/AA:BB:CC:DD:EE:01/registry");
        Assert.NotNull(registry);
        Assert.Equal("AA:BB:CC:DD:EE:01", registry.EdgeDeviceMac);
        var registeredDevice = Assert.Single(registry.Registry);
        Assert.Equal("11:22:33:44:55:66", registeredDevice.IotDeviceMac);
        Assert.Equal(42, registeredDevice.OwnerUserId);
        Assert.Equal(27, registeredDevice.PlantationId);
        Assert.Equal(88, registeredDevice.SectorId);
        Assert.False(registeredDevice.IsProvisioned);
        Assert.True(registeredDevice.IsActive);

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        var published = false;
        for (var attempt = 0; attempt < 10 && !published; attempt++)
        {
            await using var command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM device.outbox_messages WHERE processed_on_utc IS NOT NULL",
                connection);
            published = Convert.ToInt64(await command.ExecuteScalarAsync()) == 2;
            if (!published) await Task.Delay(TimeSpan.FromSeconds(1));
        }
        Assert.True(published, "The two Device outbox messages were not published within the retry window.");
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task RegisterIotDevice_WithDifferentOwnerThanGateway_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());

        const string gatewayMac = "AA:BB:CC:DD:EE:02";
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/edge-gateways", new
        {
            edgeMac = gatewayMac,
            ownerUserId = 42,
            monitoringZoneId = 9
        })).StatusCode);

        var response = await client.PostAsJsonAsync($"/api/v1/edge-gateways/{gatewayMac}/iot-devices", new
        {
            iotMac = "11:22:33:44:55:67",
            ownerUserId = 84,
            plantationId = 27,
            sectorId = 88
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task DuplicateGateway_ReturnsConflictAndKeepsSingleRow()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());
        var payload = new { edgeMac = "AA:BB:CC:DD:EE:09", ownerUserId = 42, monitoringZoneId = 1 };

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/edge-gateways", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/edge-gateways", payload)).StatusCode);
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task DeleteGatewayWithDevices_ReleasesMacForReuse()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());

        const string gatewayMac = "AA:BB:CC:DD:EE:11";
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/v1/admin/edge-gateways", new
        {
            edgeMac = gatewayMac,
            ownerUserId = 42,
            monitoringZoneId = 1
        })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
            $"/api/v1/admin/edge-gateways/{gatewayMac}/iot-devices", new
            {
                iotMac = "11:22:33:44:55:77",
                ownerUserId = 42,
                plantationId = 27,
                sectorId = 88
            })).StatusCode);

        var deletion = await client.DeleteAsync($"/api/v1/admin/edge-gateways?mac={gatewayMac}");
        Assert.Equal(HttpStatusCode.NoContent, deletion.StatusCode);

        var reRegistration = await client.PostAsJsonAsync("/api/v1/admin/edge-gateways", new
        {
            edgeMac = gatewayMac,
            ownerUserId = 84,
            monitoringZoneId = 2
        });
        Assert.Equal(HttpStatusCode.Created, reRegistration.StatusCode);
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task DeleteGatewayWithoutAdministratorRole_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/v1/admin/edge-gateways?mac=AA:BB:CC:DD:EE:12");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task DeleteMissingGateway_ReturnsNotFound()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdministratorToken());

        var response = await client.DeleteAsync("/api/v1/admin/edge-gateways?mac=AA:BB:CC:DD:EE:13");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task RegisterGateway_WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(rabbitEnabled: false);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/edge-gateways", new
        {
            edgeMac = "AA:BB:CC:DD:EE:10",
            monitoringZoneId = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory(bool rabbitEnabled) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DeviceDatabase", _postgres.GetConnectionString());
            builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests");
            builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("RabbitMq:Enabled", rabbitEnabled.ToString());
            builder.UseSetting("RabbitMq:Uri", _rabbitMq.GetConnectionString());
        });

    private static string CreateAdministratorToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "smartpalm.identity.tests",
            audience: "smartpalm.clients.tests",
            claims:
            [
                new Claim("sub", "42"),
                new Claim("sid", "42"),
                new Claim("name", "integration-admin"),
                new Claim("role", "Administrator")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
