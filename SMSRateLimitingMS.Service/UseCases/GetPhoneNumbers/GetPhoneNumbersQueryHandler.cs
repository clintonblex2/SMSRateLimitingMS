using MediatR;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;

namespace SMSRateLimitingMS.Application.UseCases.GetPhoneNumbers
{
    public class GetPhoneNumbersQueryHandler(
        IRateLimitHistoryRepository logRepository) : IRequestHandler<GetPhoneNumbersQuery, IEnumerable<string>>
    {
        private readonly IRateLimitHistoryRepository _historyRepository = logRepository;

        public async Task<IEnumerable<string>> Handle(GetPhoneNumbersQuery request, CancellationToken cancellationToken)
        {
            // Get message rate for the last 24 hours
            var messageRates = await _historyRepository.GetMessageRatesAsync(
                phoneNumber: null, // Retrieve all phone numbers
                startTime: DateTime.UtcNow.AddHours(-24), 
                endTime: DateTime.UtcNow,
                aggregationWindow: TimeSpan.FromSeconds(1),
                cancellationToken);

            // Get phone numbers excluding the account rate limit
            var phoneNumbers = messageRates
                .Where(r => r.PhoneNumber != Constants.GLOBAL_ACCOUNT)
                .Select(r => r.PhoneNumber)
                .Distinct()
                .OrderBy(o => o)
                .ToList();

            return phoneNumbers;
        }
    }
}
