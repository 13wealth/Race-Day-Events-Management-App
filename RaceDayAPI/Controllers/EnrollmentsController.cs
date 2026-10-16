using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.DTO;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayContext _db;

        public EnrollmentsController(RaceDayContext db)
        {
            _db = db;
        }

        // Finds the participant record for the logged-in user
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

        // Finds the organiser record for the logged-in user
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

        // Builds the JSON shape returned for one enrolment
        private object EnrollmentToResponse(Enrollment en)
        {
            return new
            {
                enrolment_id = en.EnrollmentId,
                participant_id = en.ParticipantId,
                event_id = en.EventId,
                category_id = en.CategoryId,
                entry_date = en.EntryDate,
                status = en.Status
            };
        }

        // POST api/enrolments (participant only)
        [HttpPost("enrolments")]
        [RequireRole("participant")]
        public IActionResult EnrolInEvent([FromBody] EnrollmentRequest request)
        {
            Participant participant = GetLoggedInParticipant();
            if (participant == null)
            {
                return NotFound(new { error = "Participant profile not found." });
            }

            // The category tells us which event the participant is entering
            Category category = (from c in _db.Categories
                                 where c.CategoryId == request.CategoryId
                                 select c).FirstOrDefault();

            if (category == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            // A participant can enter the same event only once
            Enrollment existing = (from en in _db.Enrollments
                                   where en.ParticipantId == participant.ParticipantId
                                       && en.EventId == category.EventId
                                   select en).FirstOrDefault();

            if (existing != null)
            {
                return Conflict(new { error = "You are already enrolled in this event." });
            }

            Enrollment enrollment = new Enrollment();
            enrollment.ParticipantId = participant.ParticipantId;
            enrollment.EventId = category.EventId;
            enrollment.CategoryId = category.CategoryId;
            enrollment.EntryDate = DateTime.UtcNow;
            enrollment.Status = "registered";

            _db.Enrollments.Add(enrollment);
            _db.SaveChanges();

            return StatusCode(201, EnrollmentToResponse(enrollment));
        }

        // GET api/enrolments/mine (participant only)
        [HttpGet("enrolments/mine")]
        [RequireRole("participant")]
        public IActionResult GetMyEnrollments()
        {
            Participant participant = GetLoggedInParticipant();
            if (participant == null)
            {
                return NotFound(new { error = "Participant profile not found." });
            }

            List<Enrollment> mine = (from en in _db.Enrollments
                                     where en.ParticipantId == participant.ParticipantId
                                     select en).ToList();

            List<object> result = new List<object>();
            foreach (Enrollment en in mine)
            {
                result.Add(EnrollmentToResponse(en));
            }

            return Ok(result);
        }

        // GET api/events/5/enrolments (organiser who owns the event only)
        [HttpGet("events/{eventId}/enrolments")]
        [RequireRole("organiser")]
        public IActionResult GetEventEnrollments(int eventId)
        {
            Organiser organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            RaceEvent found = (from e in _db.Events
                               where e.EventId == eventId
                               select e).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only view enrolments for your own events." });
            }

            List<Enrollment> enrollments = (from en in _db.Enrollments
                                            where en.EventId == eventId
                                            select en).ToList();

            List<object> result = new List<object>();
            foreach (Enrollment en in enrollments)
            {
                result.Add(EnrollmentToResponse(en));
            }

            return Ok(result);
        }
    }
}