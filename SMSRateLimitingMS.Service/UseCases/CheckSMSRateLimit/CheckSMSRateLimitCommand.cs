using MediatR;
using SMSRateLimitingMS.Application.Models.DTOs;

namespace SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit
{
    public record CheckSMSRateLimitCommand(string BusinessPhoneNumber) : IRequest<CheckSMSRateLimitDto>;
}
