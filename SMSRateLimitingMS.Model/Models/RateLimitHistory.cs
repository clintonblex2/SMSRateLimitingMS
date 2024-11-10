namespace SMSRateLimitingMS.Domain.Models
{
    public record RateLimitHistory
    {
        public RateLimitHistory(string phoneNumber, DateTime timestamp, bool wasAllowed, string? denialReason)
        {
            PhoneNumber = phoneNumber;
            Timestamp = timestamp;
            WasAllowed = wasAllowed;
            DenialReason = denialReason;
        }

        public string Id { get; } = Guid.NewGuid().ToString();
        public string PhoneNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public bool WasAllowed { get; set; }
        public string? DenialReason { get; set; }
    }
}
