using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Application.Models.DTOs
{
    public record RateLimitLogDto
    {
        public RateLimitLogDto(RateLimitHistory log)
        {
            Id = log.Id;
            PhoneNumber = log.PhoneNumber;
            Timestamp = log.Timestamp;
            WasAllowed = log.WasAllowed;
            DenialReason = log.DenialReason;
        }

        public string Id { get; init; }
        public string PhoneNumber { get; init; }
        public DateTime Timestamp { get; init; }
        public bool WasAllowed { get; init; }
        public string? DenialReason { get; init; }
    }
}
