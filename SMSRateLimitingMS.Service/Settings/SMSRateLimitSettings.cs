namespace SMSRateLimitingMS.Application.Settings
{
    public class SMSRateLimitSettings
    {
        public int MaxMessagesPerBusinessPhoneNumberPerSecond { get; set; }
        public int MaxMessagesPerAccountPerSecond { get; set; }
        public TimeSpan InactiveThreshold { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan HistoryRetentionPeriod { get; set; } = TimeSpan.FromMinutes(15);
    }
}
