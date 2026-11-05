using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Application.Abstractions;
using SmartPalm.DeviceService.Application.Services;
using SmartPalm.DeviceService.Domain.Repositories;
using SmartPalm.DeviceService.Infrastructure.Configuration;
using SmartPalm.DeviceService.Infrastructure.Messaging;
using SmartPalm.DeviceService.Infrastructure.Persistence;

namespace SmartPalm.DeviceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DeviceDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:DeviceDatabase is required.");
        services.AddDbContext<DeviceDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IEdgeGatewayRepository, EdgeGatewayRepository>();
        services.AddScoped<IIotDeviceRepository, IotDeviceRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<DeviceDbContext>());
        services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>();
        services.AddScoped<DeviceCommandService>();
        services.AddScoped<DeviceQueryService>();
        services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
