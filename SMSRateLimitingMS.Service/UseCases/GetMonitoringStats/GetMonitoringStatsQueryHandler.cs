using MediatR;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Models.DTOs;
using SMSRateLimitingMS.Application.Models.Statistics;
using SMSRateLimitingMS.Application.Settings;

namespace SMSRateLimitingMS.Application.UseCases.GetMonitoringStats
{
    public class GetMonitoringStatsQueryHandler(
        IRateLimitHistoryRepository historyRepository,
        IRateLimitRepository rateLimitRepository,
       SMSRateLimitSettings settings,
       ILogger<GetMonitoringStatsQueryHandler> logger) :
        IRequestHandler<GetMonitoringStatsQuery, MonitoringStatsDto?>
    {
        private readonly ILogger<GetMonitoringStatsQueryHandler> _logger = logger;
        private readonly IRateLimitHistoryRepository _historyRepository = historyRepository;
        private readonly IRateLimitRepository _rateLimitRepository = rateLimitRepository;
        private readonly SMSRateLimitSettings _settings = settings;

        public async Task<MonitoringStatsDto?> Handle(
            GetMonitoringStatsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get message rates from history
                var messageRates = await _historyRepository.GetMessageRatesAsync(
                    request.PhoneNumber ?? null, // Use account if no phone number specified
                    request.StartTime,
                    request.EndTime,
                    TimeSpan.FromSeconds(1),
                    cancellationToken);

                // Get summary statistics from history
                var summary = await _historyRepository.GetSummaryStatisticsAsync(
                    request.PhoneNumber ?? null,
                    request.StartTime,
                    request.EndTime,
                    cancellationToken);

                // Get current rate limit configuration and state
                var rateLimitId = request.PhoneNumber ?? Constants.GLOBAL_ACCOUNT;
                var maxRequests = request.PhoneNumber != null
                    ? _settings.MaxMessagesPerBusinessPhoneNumberPerSecond
                    : _settings.MaxMessagesPerAccountPerSecond;

                var currentRateLimit = await _rateLimitRepository.GetOrCreateAsync(
                    rateLimitId,
                    maxRequests,
                    TimeSpan.FromSeconds(1));

                var currentStats = currentRateLimit.GetStats();

                var ratesBySecond = messageRates
                    .GroupBy(mr => mr.TimeWindow)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Sum(x => x.TotalRequests)
                    );

                // Create response DTO
                var response = new MonitoringStatsDto(
                    // Convert message rates to dictionary format
                    MessagesPerSecond: ratesBySecond,
                    // Current state
                    CurrentStats: new CurrentRateLimitStats(
                        CurrentCount: currentStats.CurrentCount,
                        RemainingCapacity: currentStats.RemainingCapacity,
                        MaximumRequests: currentStats.MaxRequests,
                        WindowDuration: currentStats.WindowDuration),

                    // Historical statistics
                    HistoricalStats: new HistoricalStats(
                        TotalRequests: summary.TotalRequests,
                        TotalBlocked: summary.RejectedRequests,
                        AverageRequestsPerSecond: summary.AverageRequestsPerSecond,
                        PeakRequestsPerSecond: summary.PeakRequestsPerSecond,
                        PeakTime: summary.PeakTime),

                    PhoneNumber: request.PhoneNumber,
                    TimeRange: new TimeRange(request.StartTime, request.EndTime));

                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving monitoring stats for {phoneNumber}", request.PhoneNumber ?? "Account");
            }

            return default;
        }
    }
}
