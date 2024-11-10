namespace SMSRateLimitingMS.Application.Settings
{
    public class SMSRateLimitSettings
    {
        public int MaxMessagesPerBusinessPhoneNumberPerSecond { get; set; }
        public int MaxMessagesPerAccountPerSecond { get; set; }
        //public TimeSpan InactiveThreshold { get; set; } = TimeSpan.FromHours(24);
        //public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);
        //public TimeSpan HistoryRetentionPeriod { get; set;} = TimeSpan.FromDays(7);
        public TimeSpan InactiveThreshold { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan HistoryRetentionPeriod { get; set; } = TimeSpan.FromMinutes(3);
    }
}
