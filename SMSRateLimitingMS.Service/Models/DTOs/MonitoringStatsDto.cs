using SMSRateLimitingMS.Application.Models.Statistics;

namespace SMSRateLimitingMS.Application.Models.DTOs
{
    public record MonitoringStatsDto(
            Dictionary<DateTime, int> MessagesPerSecond,
            CurrentRateLimitStats CurrentStats,
            HistoricalStats HistoricalStats,
            string? PhoneNumber,
            TimeRange TimeRange);

    public record TimeRange(DateTime StartTime, DateTime EndTime)
    {
        public TimeSpan Duration => EndTime - StartTime;
    }
}
