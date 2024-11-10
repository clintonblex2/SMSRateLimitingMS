using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Application.Interfaces
{
    public interface IRateLimitHistoryRepository
    {
        // Add new entry
        Task RecordMessageRateAsync(
            string phoneNumber,
            DateTime timestamp,
            bool wasSuccessful,
            CancellationToken cancellationToken = default);

        // Get aggregated rates
        Task<IReadOnlyList<MessageRateAggregate>> GetMessageRatesAsync(
            string? phoneNumber,
            DateTime startTime,
            DateTime endTime,
            TimeSpan aggregationWindow,
            CancellationToken cancellationToken = default);

        // Get summary statistics
        Task<RateLimitHistorySummary> GetSummaryStatisticsAsync(
            string? phoneNumber,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default);

        // Expired data cleanup
        Task CleanupHistoryAsync(
            DateTime cutoffTime,
            CancellationToken cancellationToken = default);
    }
}
