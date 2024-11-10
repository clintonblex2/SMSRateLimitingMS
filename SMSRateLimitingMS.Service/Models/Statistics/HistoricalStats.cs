namespace SMSRateLimitingMS.Application.Models.Statistics
{
    public record HistoricalStats(
        int TotalRequests,
        int TotalBlocked,
        double AverageRequestsPerSecond,
        double PeakRequestsPerSecond,
        DateTime? PeakTime)
    {
        public double BlockedPercentage =>
            TotalRequests > 0 ? (TotalBlocked * 100.0) / TotalRequests : 0;
    }
}
