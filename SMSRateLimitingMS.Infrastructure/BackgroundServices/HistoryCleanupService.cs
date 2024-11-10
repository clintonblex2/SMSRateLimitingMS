using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;

namespace SMSRateLimitingMS.Infrastructure.BackgroundServices
{
    public class HistoryCleanupService(
        IRateLimitHistoryRepository historyRepository,
        ILogger<HistoryCleanupService> logger,
        SMSRateLimitSettings settings) : BackgroundService
    {
        private readonly IRateLimitHistoryRepository _historyRepository = historyRepository;
        private readonly ILogger<HistoryCleanupService> _logger = logger;
        private readonly SMSRateLimitSettings _settings = settings;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cutoffTime = DateTime.UtcNow - _settings.HistoryRetentionPeriod;
                    await _historyRepository.CleanupHistoryAsync(cutoffTime, stoppingToken);
                    await Task.Delay(_settings.CleanupInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during history cleanup");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
