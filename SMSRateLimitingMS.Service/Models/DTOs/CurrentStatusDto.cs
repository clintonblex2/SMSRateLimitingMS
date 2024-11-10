namespace SMSRateLimitingMS.Application.Models.DTOs
{
    public record CurrentStatusDto(
        string PhoneNumber,
        int CurrentCount,
        int RemainingCapacity,
        int MaximumRequests,
        TimeSpan WindowDuration,
        DateTime LastAccessed)
    {
        public double UtilizationPercentage =>
            MaximumRequests > 0 ? (CurrentCount * 100.0) / MaximumRequests : 0;

        public bool IsApproachingLimit =>
            UtilizationPercentage >= 80;

        public string Status =>
            CurrentCount >= MaximumRequests
            ? "Blocked"
            : IsApproachingLimit
            ? "Warning" : "Normal";
    }
}
