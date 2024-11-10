using MediatR;
using SMSRateLimitingMS.Application.Models.DTOs;

namespace SMSRateLimitingMS.Application.UseCases.GetCurrentStatus
{
    public record GetCurrentStatusQuery : IRequest<IEnumerable<CurrentStatusDto>>;
}
