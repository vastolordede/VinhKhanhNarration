using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DAO;

namespace VinhKhanhNarration.Api.Services;

public class LifecycleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LifecycleHostedService> _logger;

    public LifecycleHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<LifecycleHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnce(stoppingToken);
        var intervalMinutes = int.TryParse(
            _configuration["Lifecycle:IntervalMinutes"], out var configured)
            ? Math.Clamp(configured, 1, 1440)
            : 15;
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RunOnce(stoppingToken);
    }

    private Task RunOnce(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return Task.CompletedTask;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var vendor = scope.ServiceProvider.GetRequiredService<VendorModuleBUS>();
            var guest = scope.ServiceProvider.GetRequiredService<GuestAccessBUS>();
            var vendorRefresh = scope.ServiceProvider.GetRequiredService<VendorRefreshTokenDAO>();
            var adminRefresh = scope.ServiceProvider.GetRequiredService<AdminRefreshTokenDAO>();
            var audit = scope.ServiceProvider.GetRequiredService<AuditLogBUS>();
            var geofence = scope.ServiceProvider.GetRequiredService<GeofenceBUS>();

            var vendorsChecked = vendor.RefreshAllLifecycles();
            var guestRows = guest.ExpireStaleRows();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var vendorRefreshDeleted = vendorRefresh.DeleteExpiredAndRevoked(cutoff);
            var adminRefreshDeleted = adminRefresh.DeleteExpiredAndRevokedTokens(cutoff);
            var geofenceRetentionDays = int.TryParse(
                _configuration["Lifecycle:GeofenceRetentionDays"], out var configuredRetention)
                ? Math.Clamp(configuredRetention, 7, 3650)
                : 90;
            var geofenceDeleted = geofence.CleanupOlderThan(
                DateTime.UtcNow.AddDays(-geofenceRetentionDays));
            audit.Write(
                "System",
                null,
                null,
                "LifecycleMaintenance",
                "Scheduler",
                null,
                new
                {
                    VendorsChecked = vendorsChecked,
                    GuestRowsExpired = guestRows,
                    VendorRefreshTokensDeleted = vendorRefreshDeleted,
                    AdminRefreshTokensDeleted = adminRefreshDeleted,
                    GeofenceEventsDeleted = geofenceDeleted
                },
                null,
                "LifecycleHostedService");
            _logger.LogInformation(
                "Lifecycle maintenance completed: vendors={Vendors}, guestRows={GuestRows}",
                vendorsChecked,
                guestRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lifecycle maintenance failed.");
        }
        return Task.CompletedTask;
    }
}
