using System.ComponentModel.DataAnnotations;

namespace SMSRateLimitingMS.Application.Models.Requests
{
    public class GetStatisticsRequest
    {
        public DateTime StartTime { get; set; } = DateTime.UtcNow.AddDays(-1);
        public DateTime EndTime { get; set; } = DateTime.UtcNow;

        [RegularExpression(@"^\+[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in E.164 format")]
        public string? BusinessPhoneNumber { get; set; }
    }
}
