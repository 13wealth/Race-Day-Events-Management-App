using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EnrollmentService _enrollmentService;

        public EnrollmentsController(EnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

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
