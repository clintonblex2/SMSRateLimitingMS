using MediatR;
using SMSRateLimitingMS.Application.Models.DTOs;

namespace SMSRateLimitingMS.Application.UseCases.GetMonitoringStats
{
    public record GetMonitoringStatsQuery(
        DateTime StartTime,
        DateTime EndTime,
        string? PhoneNumber = null) : IRequest<MonitoringStatsDto?>;
}
