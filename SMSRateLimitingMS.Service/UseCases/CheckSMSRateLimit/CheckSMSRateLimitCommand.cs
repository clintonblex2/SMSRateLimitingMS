using MediatR;

namespace SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit
{
    public record CheckSMSRateLimitCommand(string BusinessPhoneNumber) : IRequest<SMSRateLimitResult>;
}
