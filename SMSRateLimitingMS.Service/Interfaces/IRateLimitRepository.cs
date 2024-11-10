using SMSRateLimitingMS.Domain.Entities;
using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Application.Interfaces
{
    public interface IRateLimitRepository
    {
        Task<RateLimit> GetOrCreateAsync(string id, int maximumRequests, TimeSpan window);
        Task CleanupInactiveAsync(TimeSpan threshold);
        Task<CounterStatistics?> GetRateLimitStatistics(string id);
        Task<IEnumerable<RateLimit>> GetActiveRateLimits(TimeSpan? activeThreshold = null);
    }
}
