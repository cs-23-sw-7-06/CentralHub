namespace CentralHub.Api.Services;

public interface IScopedProcessingService
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}