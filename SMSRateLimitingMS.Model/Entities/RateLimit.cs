using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Domain.Entities
{
    public class RateLimit(string id, int maximumRequests, TimeSpan window)
    {
        private readonly SlidingWindowCounter _windowCounter = new(window, maximumRequests);

        public string Id { get; } = id;
        public DateTime LastAccessed { get; private set; } = DateTime.UtcNow;

        public bool TryIncrementWindowCounter()
        {
            LastAccessed = DateTime.UtcNow;
            return _windowCounter.TryIncrementCounter();
        }

        public void DecrementCounter()
        {
            _windowCounter.Decrement();
        }

        public CounterStatistics GetStats()
        {
            LastAccessed = DateTime.UtcNow;
            return _windowCounter.GetStats();
        }

        public bool IsInactive(TimeSpan threshold)
        {
            return DateTime.UtcNow - LastAccessed > threshold;
        }
    }
}
