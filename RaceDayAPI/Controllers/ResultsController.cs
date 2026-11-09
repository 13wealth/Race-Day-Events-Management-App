using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages race results.
    /// Provides endpoints for recording and retrieving participant race results.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly ResultService _resultService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsController"/> class.
        /// </summary>
        /// <param name="resultService">The result service.</param>
        public ResultsController(ResultService resultService)
        {
            _resultService = resultService;
        }

        /// <summary>
        /// Records the race result for a participant's enrollment.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="enrolmentId">The ID of the enrollment.</param>
        /// <param name="request">The result request containing start time, finish time, and position.</param>
        /// <returns>
        /// A 201 Created response with the result details on success,
        /// a 400 Bad Request if validation fails,
        /// a 403 Forbidden if user is not the event organizer,
        /// a 404 Not Found if the enrollment doesn't exist,
        /// or a 409 Conflict if a result already exists for the enrollment.
        /// </returns>
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

        /// <summary>
        /// Retrieves a specific race result by ID.
        /// </summary>
        /// <param name="resultId">The ID of the result.</param>
        /// <returns>
        /// A 200 OK response with the result details,
        /// a 403 Forbidden if user does not have access to the result,
        /// or a 404 Not Found if the result doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Retrieves all race results belonging to the logged-in participant.
        /// Only authenticated participants can access their own results.
        /// </summary>
        /// <returns>A list of the participant's race results.</returns>
        [HttpGet("results/mine")]
        [RequireRole("participant")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetMyResults()
        {
            List<ResultService.ParticipantResultResponse> results =
                _resultService.GetMyResults();

            return Ok(results.Select(result => new
            {
                results_id = result.ResultsId,
                enrolment_id = result.EnrollmentId,
                event_name = result.EventName,
                event_date = result.EventDate,
                category_name = result.CategoryName,
                start_time = result.StartTime,
                finish_time = result.FinishTime,
                position = result.Position
            }));
        }
    }
}
