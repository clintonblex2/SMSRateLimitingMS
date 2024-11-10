using MediatR;

namespace SMSRateLimitingMS.Application.UseCases.GetPhoneNumbers
{
    public record GetPhoneNumbersQuery : IRequest<IEnumerable<string>>;
}
