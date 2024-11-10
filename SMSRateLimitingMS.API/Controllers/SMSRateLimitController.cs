using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMSRateLimitingMS.Application.Models.Requests;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using System.Net;

namespace SMSRateLimitingMS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SMSRateLimitController(IMediator _mediator, ILogger<SMSRateLimitController> _logger) : ControllerBase
    {
        [HttpPost(Name = "check-sms-rate-limit")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(SMSRateLimitResult))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(SMSRateLimitResult))]
        public async Task<IActionResult> CheckSMSRateLimit([FromBody] CheckRateLimitRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new CheckSMSRateLimitCommand(request.BusinessPhoneNumber), cancellationToken);
                if (result.CanSendSMS)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred on check sms rate limit for {PhoneNumber}", request.BusinessPhoneNumber);
                return StatusCode(500, "An error occurred while checking rate limit");
            }
        }
    }
}
