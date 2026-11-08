using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly ResultService _resultService;

        public ResultsController(ResultService resultService)
        {
            _resultService = resultService;
        }

        [HttpPost("enrolments/{enrolmentId}/result")]
        [RequireRole("organiser")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public IActionResult RecordResult(int enrolmentId, [FromBody] ResultRequest request)
        {
            ResultService.ResultCreateResponse response = _resultService.RecordResult(enrolmentId, request);

            if (response.StatusCode == 400)
            {
                return BadRequest(new { error = response.Error });
            }

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            if (response.StatusCode == 409)
            {
                return Conflict(new { error = response.Error });
            }

            return StatusCode(201, new
            {
                results_id = response.ResultsId,
                enrolment_id = response.EnrollmentId,
                start_time = response.StartTime,
                finish_time = response.FinishTime,
                position = response.Position
            });
        }

        [HttpGet("results/{resultId}")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetResult(int resultId)
        {
            ResultService.ResultDetailResponse response = _resultService.GetResult(resultId);

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(new
            {
                results_id = response.ResultsId,
                enrolment_id = response.EnrollmentId,
                start_time = response.StartTime,
                finish_time = response.FinishTime,
                position = response.Position
            });
        }
    }
}
