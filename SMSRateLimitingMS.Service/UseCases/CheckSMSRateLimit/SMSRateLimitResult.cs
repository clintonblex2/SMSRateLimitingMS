namespace SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit
{
    public record SMSRateLimitResult(bool CanSendSMS, string? ReasonForDenial = null);
}
