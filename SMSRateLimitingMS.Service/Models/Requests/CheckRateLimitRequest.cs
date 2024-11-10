using System.ComponentModel.DataAnnotations;

namespace SMSRateLimitingMS.Application.Models.Requests
{
    public class CheckRateLimitRequest
    {
        [Required]
        [RegularExpression(@"^\+[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in E.164 format")]
        public required string BusinessPhoneNumber { get; set; }
    }
}
