using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages participant enrollments in events.
    /// Provides endpoints for enrolling participants and retrieving enrollment information.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EnrollmentService _enrollmentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnrollmentsController"/> class.
        /// </summary>
        /// <param name="enrollmentService">The enrollment service.</param>
        public EnrollmentsController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// Enrolls a participant in an event category.
        /// Only participants can perform this action.
        /// </summary>
        /// <param name="request">The enrollment request containing participant and event details.</param>
        /// <returns>
        /// A 201 Created response with the enrollment details on success,
        /// a 404 Not Found if the event or category doesn't exist,
        /// or a 409 Conflict if the participant is already enrolled in the category.
        /// </returns>
        [HttpPost("enrolments")]
        [RequireRole("participant")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public IActionResult EnrolInEvent([FromBody] EnrollmentRequest request)
        {
            EnrollmentService.EnrollmentCreateResponse response = _enrollmentService.EnrolInEvent(request);

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
                enrolment_id = response.EnrollmentId,
                participant_id = response.ParticipantId,
                event_id = response.EventId,
                category_id = response.CategoryId,
                entry_date = response.EntryDate,
                status = response.Status
            });
        }

        /// <summary>
        /// Retrieves all enrollments for the currently authenticated participant.
        /// Only participants can perform this action.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the list of participant's enrollments,
        /// or a 404 Not Found if no enrollments exist.
        /// </returns>
        [HttpGet("enrolments/mine")]
        [RequireRole("participant")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetMyEnrollments()
        {
            EnrollmentService.EnrollmentListResponse response = _enrollmentService.GetMyEnrollments();

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(response.Enrollments);
        }

        /// <summary>
        /// Retrieves all enrollments for a specific event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>
        /// A 200 OK response with the list of event enrollments,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
        [HttpGet("events/{eventId}/enrolments")]
        [RequireRole("organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetEventEnrollments(int eventId)
        {
            EnrollmentService.EnrollmentEventListResponse response = _enrollmentService.GetEventEnrollments(eventId);

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(response.Enrollments);
        }
    }
}
