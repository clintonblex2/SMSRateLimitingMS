namespace SMSRateLimitingMS.Application.Models.Statistics
{
    public record CurrentRateLimitStats(
        int CurrentCount,
        int RemainingCapacity,
        int MaximumRequests,
        TimeSpan WindowDuration)
    {
        public double UtilizationPercentage =>
            MaximumRequests > 0 ? (CurrentCount * 100.0) / MaximumRequests : 0;
    }
}
