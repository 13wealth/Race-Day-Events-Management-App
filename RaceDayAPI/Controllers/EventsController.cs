using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTO;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using EventModel = RaceDayAPI.Models.RaceEvent;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _db;

        public EventsController(RaceDayContext db)
        {
            _db = db;
        }

        // Finds the organiser record for the logged-in user.
        // Returns null if no organiser record exists.
        private Organiser? GetLoggedInOrganiser()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return null;
            }

            Organiser? organiser = (from o in _db.Organisers
                                   join p in _db.Profiles on o.ProfileId equals p.ProfileId
                                   where p.UserId == userId
                                   select o).FirstOrDefault();

            return organiser;
        }

        // Checks the event fields. Returns an error message, or null if all fields are valid.
        private string CheckEventFields(EventRequest request)
        {
            if (request.EventType != "run" && request.EventType != "walk" && request.EventType != "cycle")
            {
                return "Event type must be run, walk or cycle.";
            }

            if (request.Status != "scheduled" && request.Status != "ongoing"
                && request.Status != "completed" && request.Status != "cancelled")
            {
                return "Status must be scheduled, ongoing, completed or cancelled.";
            }

            return null;
        }

        // Builds the JSON shape returned for one event
        private object EventToResponse(EventModel e)
        {
            return new
            {
                event_id = e.EventId,
                event_name = e.EventName,
                description = e.Description,
                location = e.Location,
                latitude = e.Latitude,
                longitude = e.Longitude,
                event_date = e.EventDate,
                distance = e.Distance,
                event_type = e.EventType,
                status = e.Status
            };
        }

        // POST api/events (organiser only)
        [HttpPost]
        [RequireRole("organiser")]
        public IActionResult CreateEvent([FromBody] EventRequest request)
        {
            Organiser? organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            string problem = CheckEventFields(request);
            if (problem != null)
            {
                return BadRequest(new { error = problem });
            }

            EventModel newEvent = new EventModel();
            newEvent.OrganiserId = organiser!.OrganiserId;
            newEvent.EventName = request.EventName;
            newEvent.Description = request.Description;
            newEvent.Location = request.Location;
            newEvent.Latitude = request.Latitude;
            newEvent.Longitude = request.Longitude;
            newEvent.EventDate = request.EventDate;
            newEvent.Distance = request.Distance;
            newEvent.EventType = request.EventType;
            newEvent.Status = request.Status;
            newEvent.CreatedAt = DateTime.UtcNow;

            _db.Events.Add(newEvent);
            _db.SaveChanges();

            return StatusCode(201, EventToResponse(newEvent));
        }

        // GET api/events (any logged-in user)
        [HttpGet]
        [RequireRole]
        public IActionResult GetEvents()
        {
            List<EventModel> events = (from e in _db.Events select e).ToList();

            List<object> result = new List<object>();
            foreach (EventModel e in events)
            {
                result.Add(EventToResponse(e));
            }

            return Ok(result);
        }

        // GET api/events/5 (any logged-in user)
        [HttpGet("{eventId}")]
        [RequireRole]
        public IActionResult GetEvent(int eventId)
        {
            EventModel? found = (from e in _db.Events
                           where e.EventId == eventId
                           select e).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            return Ok(EventToResponse(found));
        }

        // PUT api/events/5 (organiser who owns the event only)
        [HttpPut("{eventId}")]
        [RequireRole("organiser")]
        public IActionResult UpdateEvent(int eventId, [FromBody] EventRequest request)
        {
            Organiser? organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            EventModel? found = (from e in _db.Events
                           where e.EventId == eventId
                           select e).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (found!.OrganiserId != organiser!.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only change your own events." });
            }

            string problem = CheckEventFields(request);
            if (problem != null)
            {
                return BadRequest(new { error = problem });
            }

            found.EventName = request.EventName;
            found.Description = request.Description;
            found.Location = request.Location;
            found.Latitude = request.Latitude;
            found.Longitude = request.Longitude;
            found.EventDate = request.EventDate;
            found.Distance = request.Distance;
            found.EventType = request.EventType;
            found.Status = request.Status;
            found.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return Ok(EventToResponse(found));
        }

        // DELETE api/events/5 (organiser who owns the event only)
        [HttpDelete("{eventId}")]
        [RequireRole("organiser")]
        public IActionResult DeleteEvent(int eventId)
        {
            Organiser? organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            EventModel? found = (from e in _db.Events
                           where e.EventId == eventId
                           select e).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (found!.OrganiserId != organiser!.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only delete your own events." });
            }

            _db.Events.Remove(found);
            _db.SaveChanges();

            return NoContent();
        }
    }
}