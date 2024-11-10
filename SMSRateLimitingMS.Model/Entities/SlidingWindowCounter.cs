using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Domain.Entities
{
    public class SlidingWindowCounter(TimeSpan windowDuration, int maximumRequest)
    {
        private readonly Queue<DateTime> _timestamps = new();
        private readonly TimeSpan _windowDuration = windowDuration;
        private readonly int _maximumRequest = maximumRequest;
        private readonly object _lock = new();

        public bool TryIncrementCounter()
        {
            lock (_lock)
            {
                var currentCount = GetCurrentCount();
                if (currentCount >= _maximumRequest)
                {
                    return false;
                }

                _timestamps.Enqueue(DateTime.UtcNow);
                return true;
            }
        }

        public void Decrement()
        {
            lock (_lock)
            {
                if (_timestamps.Count != 0)
                {
                    _timestamps.Dequeue();
                }
            }
        }

        public CounterStatistics GetStats()
        {
            lock (_lock)
            {
                RemoveExpiredTimestamps(DateTime.UtcNow);
                
                var currentCount = _timestamps.Count;
                var remainingCapacity = _maximumRequest - currentCount;

                // Create and return the stats object
                return new CounterStatistics(currentCount, remainingCapacity, _maximumRequest, _windowDuration);
            }
        }

        public int GetCurrentCount()
        {
            lock (_lock)
            {
                RemoveExpiredTimestamps(DateTime.UtcNow);
                return _timestamps.Count;
            }
        }

        private void RemoveExpiredTimestamps(DateTime utcNow)
        {
            while (_timestamps.Count > 0 && _timestamps.Peek() <= utcNow - _windowDuration)
            {
                _timestamps.Dequeue();
            }
        }
    }
}
