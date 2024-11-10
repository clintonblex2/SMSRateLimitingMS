using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;

namespace SMSRateLimitingMS.Infrastructure.BackgroundServices
{
    public class RateLimitCleanupService(
        IRateLimitRepository smsRateLimitRepository, 
        ILogger<RateLimitCleanupService> logger, 
        SMSRateLimitSettings settings) : BackgroundService
    {
        private readonly IRateLimitRepository _smsRateLimitRepository = smsRateLimitRepository;
        private readonly ILogger<RateLimitCleanupService> _logger = logger;
        private readonly SMSRateLimitSettings _settings = settings;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _smsRateLimitRepository.CleanupInactiveAsync(_settings.InactiveThreshold);
                    await Task.Delay(_settings.CleanupInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred on SMS rate limit cleanup");

                    // Wait for a minute before retrying after an error
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
