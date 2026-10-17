using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.DTO;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using RaceResult = RaceDayAPI.Models.RaceResult;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _db;

        public ResultsController(RaceDayContext db)
        {
            _db = db;
        }

        /*
         * Helper methods to get the logged-in organiser or participant
         */
        private Organiser GetLoggedInOrganiser()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (userIdNullable == null)
            {
                return null;
            }
            int userId = userIdNullable.Value;

            Organiser organiser = (from o in _db.Organisers
                                   join p in _db.Profiles on o.ProfileId equals p.ProfileId
                                   where p.UserId == userId
                                   select o).FirstOrDefault();

            return organiser;
        }

        /*
         * Helper method to get the logged-in participant
         */
        private Participant GetLoggedInParticipant()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (userIdNullable == null)
            {
                return null;
            }
            int userId = userIdNullable.Value;

            Participant participant = (from p in _db.Participants
                                       join pr in _db.Profiles on p.ProfileId equals pr.ProfileId
                                       where pr.UserId == userId
                                       select p).FirstOrDefault();

            return participant;
        }

        /*
         * Helper method to convert a RaceResult to a response object
         */
        private object ResultToResponse(RaceResult r)
        {
            return new
            {
                results_id = r.ResultsId,
                enrolment_id = r.EnrollmentId,
                start_time = r.StartTime,
                finish_time = r.FinishTime,
                position = r.Position
            };
        }

        /// <summary>
        /// Records start time, finish time and position for an enrolment. Only the organiser who owns the event.
        /// </summary>
        [HttpPost("enrolments/{enrolmentId}/result")]
        [RequireRole("organiser")]
        public IActionResult RecordResult(int enrolmentId, [FromBody] ResultRequest request)
        {
            Organiser organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            Enrollment enrollment = (from en in _db.Enrollments
                                     where en.EnrollmentId == enrolmentId
                                     select en).FirstOrDefault();

            if (enrollment == null)
            {
                return NotFound(new { error = "Enrolment not found." });
            }

            RaceEvent ev = (from e in _db.Events
                            where e.EventId == enrollment.EventId
                            select e).FirstOrDefault();

            if (ev == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (ev.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only record results for your own events." });
            }

            RaceResult existing = (from r in _db.Results
                                   where r.EnrollmentId == enrolmentId
                                   select r).FirstOrDefault();

            if (existing != null)
            {
                return Conflict(new { error = "A result is already recorded for this enrolment." });
            }

            if (request.FinishTime < request.StartTime)
            {
                return BadRequest(new { error = "Finish time cannot be before start time." });
            }

            if (request.Position < 1)
            {
                return BadRequest(new { error = "Position must be 1 or more." });
            }

            RaceResult result = new RaceResult();
            result.EnrollmentId = enrolmentId;
            result.StartTime = request.StartTime;
            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.CreatedAt = DateTime.UtcNow;

            _db.Results.Add(result);
            _db.SaveChanges();

            return StatusCode(201, ResultToResponse(result));
        }

        /// <summary>
        /// Gets a specific result by ID. Only the participant who owns it, or the organiser who owns the event.
        /// </summary>
        [HttpGet("results/{resultId}")]
        [RequireRole]
        public IActionResult GetResult(int resultId)
        {
            RaceResult found = (from r in _db.Results
                                where r.ResultsId == resultId
                                select r).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Result not found." });
            }

            Enrollment enrollment = (from en in _db.Enrollments
                                     where en.EnrollmentId == found.EnrollmentId
                                     select en).FirstOrDefault();

            string role = HttpContext.Session.GetString("Role");

            // Participants may only see their own results
            if (role == "participant")
            {
                Participant participant = GetLoggedInParticipant();
                if (participant == null || enrollment.ParticipantId != participant.ParticipantId)
                {
                    return StatusCode(403, new { error = "You can only view your own results." });
                }
            }

            // Organisers may only see results for their own events
            if (role == "organiser")
            {
                Organiser organiser = GetLoggedInOrganiser();
                RaceEvent ev = (from e in _db.Events
                                where e.EventId == enrollment.EventId
                                select e).FirstOrDefault();

                if (organiser == null || ev == null || ev.OrganiserId != organiser.OrganiserId)
                {
                    return StatusCode(403, new { error = "You can only view results for your own events." });
                }
            }

            return Ok(ResultToResponse(found));
        }
    }
}