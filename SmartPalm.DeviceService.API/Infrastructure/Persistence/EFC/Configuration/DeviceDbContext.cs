using Microsoft.EntityFrameworkCore;
using SmartPalm.DeviceService.Domain.Devices;
using SmartPalm.DeviceService.Domain.Repositories;
using SmartPalm.DeviceService.Infrastructure.Messaging;

namespace SmartPalm.DeviceService.Infrastructure.Persistence;

public class DeviceDbContext(DbContextOptions<DeviceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<EdgeGateway> EdgeGateways => Set<EdgeGateway>();
    public DbSet<IotDevice> IotDevices => Set<IotDevice>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("device");
        modelBuilder.Entity<EdgeGateway>(entity =>
        {
            entity.ToTable("edge_gateways");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedOnAdd();
            entity.Property(value => value.MacAddress).HasMaxLength(50).IsRequired();
            entity.Property(value => value.OwnerUserId).IsRequired();
            entity.HasIndex(value => value.MacAddress).IsUnique();
            entity.HasIndex(value => value.MonitoringZoneId);
            entity.Property(value => value.CredentialHash).HasMaxLength(128).IsRequired(false);
            entity.Property(value => value.IsActive).IsRequired();
        });
        modelBuilder.Entity<IotDevice>(entity =>
        {
            entity.ToTable("iot_devices");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.Id).ValueGeneratedOnAdd();
            entity.Property(value => value.MacAddress).HasMaxLength(50).IsRequired();
            entity.Property(value => value.GatewayMacAddress).HasMaxLength(50).IsRequired();
            entity.Property(value => value.OwnerUserId).IsRequired();
            entity.HasIndex(value => value.MacAddress).IsUnique();
            entity.HasIndex(value => value.GatewayMacAddress);
            entity.HasIndex(value => value.PlantationId);
            entity.HasIndex(value => new { value.PlantationId, value.SectorId });
            entity.Property(value => value.SectorId).IsRequired();
            entity.Property(value => value.BootstrapCredentialHash).HasMaxLength(128).IsRequired(false);
            entity.Property(value => value.IsProvisioned).IsRequired();
            entity.Property(value => value.ProvisionedAt).IsRequired(false);
            entity.Property(value => value.IsActive).IsRequired();
            entity.HasOne<EdgeGateway>()
                .WithMany()
                .HasForeignKey(value => value.GatewayMacAddress)
                .HasPrincipalKey(value => value.MacAddress)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(value => value.Id);
            entity.Property(value => value.EventType).HasMaxLength(200).IsRequired();
            entity.Property(value => value.Content).HasColumnType("jsonb").IsRequired();
            entity.Property(value => value.Error).HasMaxLength(2000);
            entity.HasIndex(value => new { value.ProcessedOnUtc, value.OccurredOnUtc });
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
