namespace SMSRateLimitingMS.Domain.Models
{
    public record RateLimitHistorySummary(
        int TotalRequests,
        int RejectedRequests,
        double AverageRequestsPerSecond,
        double PeakRequestsPerSecond,
        DateTime? PeakTime);
}
