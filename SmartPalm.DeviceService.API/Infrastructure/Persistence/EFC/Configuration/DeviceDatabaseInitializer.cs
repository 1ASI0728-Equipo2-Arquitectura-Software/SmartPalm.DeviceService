using Microsoft.EntityFrameworkCore;

namespace SmartPalm.DeviceService.Infrastructure.Persistence;

public static class DeviceDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DeviceDatabaseInitializer");
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                return;
            }
            catch (Exception exception) when (attempt < 10)
            {
                logger.LogWarning(exception, "Database migration attempt {Attempt} failed; retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
