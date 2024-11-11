namespace SMSRateLimitingMS.Application.Models.DTOs
{
    public record CheckSMSRateLimitDto(bool CanSendSMS, string? ReasonForDenial = null);
}
