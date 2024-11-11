using System.ComponentModel.DataAnnotations;

namespace SMSRateLimitingMS.Application.Models.Requests
{
    public class CheckRateLimitRequest
    {
        [Required]
        [RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Phone number must be in E.164 format, starting with '+', followed by 8 to 15 digits.")]
        public required string BusinessPhoneNumber { get; set; }
    }
}
