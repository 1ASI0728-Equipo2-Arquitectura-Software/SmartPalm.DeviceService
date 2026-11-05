using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartPalm.DeviceService.Infrastructure.Persistence;

public class DeviceDbContextFactory : IDesignTimeDbContextFactory<DeviceDbContext>
{
    public DeviceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DeviceDbContext>()
            .UseNpgsql(GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;
        return new DeviceDbContext(options);
    }

    private static string GetConnectionString() => BuildConfiguration().GetConnectionString("DeviceDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:DeviceDatabase is required.");

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .SetBasePath(ResolveConfigurationPath())
            .AddJsonFile("appsettings.json", optional: false);
        if (!string.IsNullOrWhiteSpace(environment)) builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        return builder.AddEnvironmentVariables().Build();
    }

    private static string ResolveConfigurationPath()
    {
        foreach (var path in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            if (File.Exists(Path.Combine(path, "appsettings.json"))) return path;
        throw new FileNotFoundException("Unable to locate appsettings.json for EF Core design-time configuration.");
    }
}
