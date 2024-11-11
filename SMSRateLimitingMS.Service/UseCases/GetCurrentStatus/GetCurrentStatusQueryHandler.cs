using MediatR;
using Microsoft.Extensions.Logging;
using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Models.DTOs;

namespace SMSRateLimitingMS.Application.UseCases.GetCurrentStatus
{
    public class GetCurrentStatusQueryHandler(
        IRateLimitRepository rateLimitRepository,
        ILogger<GetCurrentStatusQueryHandler> logger) : IRequestHandler<GetCurrentStatusQuery, IEnumerable<CurrentStatusDto>>
    {
        private readonly IRateLimitRepository _rateLimitRepository = rateLimitRepository;
        private readonly ILogger<GetCurrentStatusQueryHandler> _logger = logger;

        public async Task<IEnumerable<CurrentStatusDto>> Handle(GetCurrentStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var activeRateLimits = await _rateLimitRepository.GetActiveRateLimits();

                var statusList = activeRateLimits
                    .Select(rl =>
                    {
                        var stats = rl.GetStats();
                        return new CurrentStatusDto
                        (
                            PhoneNumber: rl.Id,
                            CurrentCount: stats.CurrentCount,
                            RemainingCapacity: stats.RemainingCapacity,
                            MaximumRequests: stats.MaxRequests,
                            WindowDuration: stats.WindowDuration,
                            LastAccessed: rl.LastAccessed
                        );
                    })
                    .OrderByDescending(status => status.UtilizationPercentage)
                    .ToList();

                return statusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting rate limit current status");
            }

            return [];
        }
    }
}
