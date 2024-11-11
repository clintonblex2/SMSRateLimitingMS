using MediatR;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Domain.ValueObjects;

namespace SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit
{
    public class CheckSMSRateLimitCommandHandler(
        IRateLimitRepository smsRateLimitRepository,
        IRateLimitHistoryRepository rateLimitLogRepository,
        ILogger<CheckSMSRateLimitCommandHandler> logger,
        SMSRateLimitSettings settings)
        : IRequestHandler<CheckSMSRateLimitCommand, SMSRateLimitResult>
    {
        private readonly IRateLimitRepository _smsRateLimitRepository = smsRateLimitRepository;
        private readonly IRateLimitHistoryRepository _rateLimitLogRepository = rateLimitLogRepository;
        private readonly ILogger<CheckSMSRateLimitCommandHandler> _logger = logger;
        private readonly SMSRateLimitSettings _settings = settings;

        public async Task<SMSRateLimitResult> Handle(CheckSMSRateLimitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var phoneNumberValidation = PhoneNumber.Create(request.BusinessPhoneNumber);
                if (!phoneNumberValidation.IsSuccessful)
                    return new SMSRateLimitResult(false, phoneNumberValidation.Error);

                // Check business phone number limit
                var phoneNumberLimit = await _smsRateLimitRepository.GetOrCreateAsync(
                    phoneNumberValidation.Value!.Value,
                    _settings.MaxMessagesPerBusinessPhoneNumberPerSecond,
                    TimeSpan.FromSeconds(1));

                // Check business phone number limit
                if (!phoneNumberLimit.TryIncrementWindowCounter())
                {
                    var stats = phoneNumberLimit.GetStats();
                    string reason = $"Phone number rate limit exceeded ({stats.CurrentCount}/{stats.MaxRequests} messages per second)";

                    // Record the attempt for phone number
                    await _rateLimitLogRepository.RecordMessageRateAsync(
                            request.BusinessPhoneNumber,
                            DateTime.UtcNow,
                            wasSuccessful: false,
                            cancellationToken);

                    return new SMSRateLimitResult(false, reason);
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
                    var reason = $"Global account rate limit exceeded ({stats.CurrentCount}/{stats.MaxRequests} messages per second)";

                    // Record the attempt for phone number
                    await _rateLimitLogRepository.RecordMessageRateAsync(
                        Constants.GLOBAL_ACCOUNT,
                        DateTime.UtcNow,
                        wasSuccessful: false,
                        cancellationToken);

                    return new SMSRateLimitResult(false, reason);
                }

                // Both limits passed
                await _rateLimitLogRepository.RecordMessageRateAsync(
                        request.BusinessPhoneNumber,
                        DateTime.UtcNow,
                        wasSuccessful: true,
                        cancellationToken);

                return new SMSRateLimitResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred on check SMS rate limit for {phoneNumber}", request.BusinessPhoneNumber);
            }

            return new SMSRateLimitResult(false, "Request processing failed.");
        }
    }
}
