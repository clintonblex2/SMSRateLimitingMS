namespace SMSRateLimitingMS.Domain.Models
{
    public record CounterStatistics(
        int CurrentCount,
        int RemainingCapacity,
        int MaxRequests,
        TimeSpan WindowDuration);
}
