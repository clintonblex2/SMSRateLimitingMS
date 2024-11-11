using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;
using System.Collections.Concurrent;

namespace SMSRateLimitingMS.Infrastructure.Persistence
{
    public class InMemoryRateLimitHistoryRepository() : IRateLimitHistoryRepository
    {
        private readonly ConcurrentDictionary<(string PhoneNumber, DateTime Window), MessageRateAggregate> _aggregates = [];

        public Task RecordMessageRateAsync(string phoneNumber, DateTime timestamp, bool wasSuccessful, CancellationToken cancellationToken = default)
        {
            var key = (phoneNumber, timestamp);

            _aggregates.AddOrUpdate(
                key,
                // Add new aggregate
                _ => new MessageRateAggregate(
                    phoneNumber,
                    timestamp,
                    TimeSpan.FromSeconds(1),
                    totalRequests: 1,
                    rejectedRequests: wasSuccessful ? 0 : 1
                ),
                // Update existing aggrgate
                (_, existing) => new MessageRateAggregate(
                    existing.PhoneNumber,
                    existing.TimeWindow,
                    existing.WindowSize,
                    existing.TotalRequests + 1,
                    existing.RejectedRequests + (wasSuccessful ? 0 : 1))
                );

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MessageRateAggregate>> GetMessageRatesAsync(string? phoneNumber, DateTime startTime, DateTime endTime, TimeSpan aggregationWindow, CancellationToken cancellationToken = default)
        {
            var query = _aggregates.Values
                .Where(a => a.TimeWindow >= startTime && a.TimeWindow <= endTime);

            if (!string.IsNullOrEmpty(phoneNumber))
                query = query.Where(a => a.PhoneNumber == phoneNumber);

            var rates = query
                .GroupBy(a => new
                {
                    a.PhoneNumber,
                    // Round to aggregation window
                    TImeWindow = new DateTime(
                        a.TimeWindow.Ticks - (a.TimeWindow.Ticks % aggregationWindow.Ticks),
                        a.TimeWindow.Kind)
                })
                .Select(s => new MessageRateAggregate(
                    phoneNumber: s.Key.PhoneNumber,
                    timeWindow: s.Key.TImeWindow,
                    windowSize: aggregationWindow,
                    totalRequests: s.Sum(a => a.TotalRequests),
                    rejectedRequests: s.Sum(s => s.RejectedRequests)))
                .OrderBy(o => o.TimeWindow)
                .ToList() as IReadOnlyList<MessageRateAggregate>;

            return Task.FromResult(rates);
        }

        public async Task<RateLimitHistorySummary> GetSummaryStatisticsAsync(string? phoneNumber, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
        {
            var rates = await GetMessageRatesAsync(
                phoneNumber,
                startTime,
                endTime,
                TimeSpan.FromSeconds(1),
                cancellationToken);

            if (!rates.Any())
                return new RateLimitHistorySummary(0, 0, 0, 0, null);

            var totalRequests = rates.Sum(r => r.TotalRequests);
            var rejectedRequests = rates.Sum(r => r.RejectedRequests);
            var timeSpan = endTime - startTime;
            var averageRate = totalRequests / timeSpan.TotalSeconds;
            var peakRate = rates.Max(r => r.TotalRequests);
            var peakTime = rates.FirstOrDefault(r => r.TotalRequests == peakRate)?.TimeWindow;

            return new RateLimitHistorySummary(
                totalRequests,
                rejectedRequests,
                averageRate,
                peakRate,
                peakTime);
        }

        public Task CleanupHistoryAsync(DateTime cutoffTime, CancellationToken cancellationToken = default)
        {
            foreach (var kvp in _aggregates)
            {
                if (kvp.Value.TimeWindow < cutoffTime)
                {
                    _aggregates.TryRemove(kvp.Key, out _);
                }
            }

            return Task.CompletedTask;
        }

        private static DateTime RoundToWindow2(DateTime timestamp)
        {
            // Round to nearest second
            var seconds = timestamp.Second;
            var roundedTimestamp = timestamp.AddSeconds(seconds - seconds % 1);
            return new DateTime(
                roundedTimestamp.Year,
                roundedTimestamp.Month,
                roundedTimestamp.Day,
                roundedTimestamp.Hour,
                roundedTimestamp.Minute,
                roundedTimestamp.Second,
                0,
                roundedTimestamp.Kind);
        }
    }
}
