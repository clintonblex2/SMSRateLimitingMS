using System.ComponentModel.DataAnnotations;

namespace SMSRateLimitingMS.Application.Models.Requests
{
    public class CheckRateLimitRequest
    {
        [Required]
        public required string BusinessPhoneNumber { get; set; }
    }
}
