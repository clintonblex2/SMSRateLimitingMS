using SMSRateLimitingMS.Application.Models.DTOs;
using SMSRateLimitingMS.Domain.Models;

namespace SMSRateLimitingMS.Application.Mapping
{
    public static class MapingExtensions
    {
        public static RateLimitLogDto ToDto(this RateLimitHistory log)
        {
            return new RateLimitLogDto(log);
        }

        public static IEnumerable<RateLimitLogDto> ToDto(this IEnumerable<RateLimitHistory> logs)
        {
            return logs.Select(log => log.ToDto());
        }
    }
}
