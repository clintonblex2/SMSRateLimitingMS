using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMSRateLimitingMS.Application.Helpers;
using SMSRateLimitingMS.Application.Models.DTOs;
using SMSRateLimitingMS.Application.Models.Requests;
using SMSRateLimitingMS.Application.UseCases.GetMonitoringStats;
using SMSRateLimitingMS.Application.UseCases.GetPhoneNumbers;
using System.Net;

namespace SMSRateLimitingMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonitoringController(IMediator _mediator, ILogger<MonitoringController> _logger) : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of phone numbers.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A collection of phone numbers:
        /// - 200 OK: Successfully retrieved the phone numbers.
        /// - 500 Internal Server Error: An error occurred while processing the request.
        /// - 400 Bad Request: The request was invalid.
        /// </returns>
        [HttpGet("phone-numbers")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetPhoneNumbers(CancellationToken cancellationToken)
        {
            try
            {
                // Send the query to get phone numbers
                var result = await _mediator.Send(new GetPhoneNumbersQuery(), cancellationToken);

                // Return the result with 200 OK status
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error getting phone numbers");

                // Return 500 Internal Server Error with a processing error message
                return StatusCode(500, Constants.PROCESSING_ERROR);
            }
        }

        /// <summary>
        /// Retrieves monitoring statistics.
        /// </summary>
        /// <param name="request">The request containing the start time, end time, and business phone number.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Monitoring statistics:
        /// - 200 OK: Successfully retrieved the statistics.
        /// - 500 Internal Server Error: An error occurred while processing the request.
        /// - 400 Bad Request: The end time is before the start time or the request is invalid.
        /// </returns>
        [HttpGet("statistics")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(MonitoringStatsDto))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetStatistics([FromQuery] GetStatisticsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the request: End time must be after the start time
                if (request.EndTime <= request.StartTime)
                    return BadRequest("End time must be after start time");

                // Send the query to get monitoring statistics
                var result = await _mediator.Send(new GetMonitoringStatsQuery(
                    request.StartTime,
                    request.EndTime,
                    request.BusinessPhoneNumber),
                    cancellationToken);

                // Return the result with 200 OK status
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception with specific context information
                _logger.LogError(ex, "Error getting monitoring stats for {phoneNumber}", request.BusinessPhoneNumber ?? Constants.GLOBAL_ACCOUNT);

                // Return 500 Internal Server Error with a processing error message
                return StatusCode(StatusCodes.Status500InternalServerError, Constants.PROCESSING_ERROR);
            }
        }

    }
}
