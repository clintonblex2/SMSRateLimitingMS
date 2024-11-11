using MediatR;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Models.DTOs;
using SMSRateLimitingMS.Application.Settings;

namespace SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit
{
    public class CheckSMSRateLimitCommandHandler(
        IRateLimitRepository smsRateLimitRepository,
        IRateLimitHistoryRepository rateLimitLogRepository,
        ILogger<CheckSMSRateLimitCommandHandler> logger,
        SMSRateLimitSettings settings)
        : IRequestHandler<CheckSMSRateLimitCommand, CheckSMSRateLimitDto>
    {
        private readonly IRateLimitRepository _smsRateLimitRepository = smsRateLimitRepository;
        private readonly IRateLimitHistoryRepository _rateLimitLogRepository = rateLimitLogRepository;
        private readonly ILogger<CheckSMSRateLimitCommandHandler> _logger = logger;
        private readonly SMSRateLimitSettings _settings = settings;

        public async Task<CheckSMSRateLimitDto> Handle(CheckSMSRateLimitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check business phone number limit
                var phoneNumberLimit = await _smsRateLimitRepository.GetOrCreateAsync(
                    request.BusinessPhoneNumber,
                    _settings.MaxMessagesPerBusinessPhoneNumberPerSecond,
                    TimeSpan.FromSeconds(1));

                // Check business phone number limit
                if (!phoneNumberLimit.TryIncrementWindowCounter())
                {
                    var stats = phoneNumberLimit.GetStats();

                    // Record the attempt for phone number
                    await _rateLimitLogRepository.RecordMessageRateAsync(
                            request.BusinessPhoneNumber,
                            DateTime.UtcNow,
                            wasSuccessful: false,
                            cancellationToken);

                    return new CheckSMSRateLimitDto(false, $"Phone number rate limit exceeded ({stats.CurrentCount}/{stats.MaxRequests} messages per second)");
                }

                // Check global account limit
                var accountLimit = await _smsRateLimitRepository.GetOrCreateAsync(
                    Constants.GLOBAL_ACCOUNT,
                    _settings.MaxMessagesPerAccountPerSecond,
                    TimeSpan.FromSeconds(1));

                // Check global account limit
                if (!accountLimit.TryIncrementWindowCounter())
                {
                    // Rollback the phone number increment since global limit failed
                    phoneNumberLimit.DecrementCounter();

                    var stats = accountLimit.GetStats();

                    // Record the attempt for phone number
                    await _rateLimitLogRepository.RecordMessageRateAsync(
                        Constants.GLOBAL_ACCOUNT,
                        DateTime.UtcNow,
                        wasSuccessful: false,
                        cancellationToken);

                    return new CheckSMSRateLimitDto(false, $"Global account rate limit exceeded ({stats.CurrentCount}/{stats.MaxRequests} messages per second)");
                }

                // Both limits passed
                await _rateLimitLogRepository.RecordMessageRateAsync(
                        request.BusinessPhoneNumber,
                        DateTime.UtcNow,
                        wasSuccessful: true,
                        cancellationToken);

                return new CheckSMSRateLimitDto(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred on check SMS rate limit for {phoneNumber}", request.BusinessPhoneNumber);
            }

            return new CheckSMSRateLimitDto(false, Constants.PROCESSING_ERROR);
        }
    }
}
