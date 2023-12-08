namespace CentralHub.Api.Services;

public sealed class ScopedBackgroundService(IServiceProvider serviceProvider,
        ILogger<ScopedBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is running.");

        await DoWorkAsync(stoppingToken);
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is working.");

        using var scope = serviceProvider.CreateScope();
        var scopedProcessingService =
            scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

        await scopedProcessingService.DoWorkAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is stopping.");

        await base.StopAsync(cancellationToken);
    }
}