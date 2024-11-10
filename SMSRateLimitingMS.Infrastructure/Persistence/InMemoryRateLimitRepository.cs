using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;
using System.Collections.Concurrent;

namespace SMSRateLimitingMS.Infrastructure.Persistence
{
    public class InMemoryRateLimitRepository : IRateLimitRepository
    {
        private readonly ConcurrentDictionary<string, RateLimit> _smsRateLimits = new();

        public Task<RateLimit> GetOrCreateAsync(string id, int maximumRequests, TimeSpan window)
        {
            var smsRateLimit = _smsRateLimits.GetOrAdd(id, _ => new RateLimit(id, maximumRequests, window));

            return Task.FromResult(smsRateLimit);
        }

        public Task<CounterStatistics?> GetRateLimitStatistics(string id)
        {
            if (_smsRateLimits.TryGetValue(id, out var rateLimits) && !rateLimits.IsInactive(TimeSpan.FromHours(24)))
            {
                return Task.FromResult<CounterStatistics?>(rateLimits.GetStats());
            }

            return Task.FromResult<CounterStatistics?>(null);
        }

        public Task<IEnumerable<RateLimit>> GetActiveRateLimits(TimeSpan? activeThreshold = null)
        {
            var threshold = activeThreshold ?? TimeSpan.FromHours(1);

            var activeRateLimits = _smsRateLimits.Values
                .Where(rl => !rl.IsInactive(threshold));

            return Task.FromResult(activeRateLimits);
        }

        public Task CleanupInactiveAsync(TimeSpan threshold)
        {
            var inactiveKeys = _smsRateLimits
                .Where(kvp => kvp.Value.IsInactive(threshold))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in inactiveKeys)
            {
                _smsRateLimits.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }
    }
}
