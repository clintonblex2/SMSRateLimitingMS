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
        [HttpGet("phone-numbers")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetPhoneNumbers(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetPhoneNumbersQuery(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting phone numbers");
                return StatusCode(500, "An error occurred while getting phone numbers");
            }
        }

        [HttpGet("statistics")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(MonitoringStatsDto))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetStatistics([FromQuery] GetStatisticsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.EndTime <= request.StartTime)
                    return BadRequest("End time must be after start time");

                var result = await _mediator.Send(new GetMonitoringStatsQuery(
                    request.StartTime,
                    request.EndTime,
                    request.BusinessPhoneNumber),
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring stats for {phoneNumber}", request.BusinessPhoneNumber ?? Constants.GLOBAL_ACCOUNT);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting monitoring stats");
            }
        }
    }
}
