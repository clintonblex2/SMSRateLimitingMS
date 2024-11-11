using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Models.DTOs;
using SMSRateLimitingMS.Application.Models.Requests;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using System.Net;

namespace SMSRateLimitingMS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SMSRateLimitController(IMediator _mediator, ILogger<SMSRateLimitController> _logger) : ControllerBase
    {
        /// <summary>
        /// Checks the SMS rate limit for a given phone number.
        /// </summary>
        /// <param name="request">The request containing the business phone number.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A CheckSMSRateLimitDto object indicating whether the SMS can be sent:
        /// - 200 OK: The SMS can be sent.
        /// - 500 Internal Server Error: An error occurred while processing the request.
        /// - 400 Bad Request: The SMS rate limit has been reached or the request is invalid.
        /// </returns>
        [HttpPost("check")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(CheckSMSRateLimitDto))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(CheckSMSRateLimitDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(CheckSMSRateLimitDto))]
        public async Task<IActionResult> CheckSMSRateLimit([FromBody] CheckRateLimitRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Send the command to check SMS rate limit
                var result = await _mediator.Send(new CheckSMSRateLimitCommand(request.BusinessPhoneNumber), cancellationToken);

                // If the SMS can be sent, return 200 OK status with the result
                if (result.CanSendSMS)
                    return Ok(result);

                // If the SMS cannot be sent, return 400 Bad Request status with the result
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                // Log the exception with specific context information
                _logger.LogError(ex, "An error occurred on check sms rate limit for {PhoneNumber}", request.BusinessPhoneNumber);

                // Return 500 Internal Server Error with a processing error message
                return StatusCode(500, Constants.PROCESSING_ERROR);
            }
        }

    }
}
