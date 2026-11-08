using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
        [RequireRole("organiser")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult CreateEvent([FromBody] EventRequest request)
        {
            EventService.EventCreateResponse response = _eventService.CreateEvent(request);

            if (response.StatusCode == 400)
            {
                return BadRequest(new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return StatusCode(201, new
            {
                event_id = response.EventId,
                event_name = response.EventName,
                description = response.Description,
                location = response.Location,
                latitude = response.Latitude,
                longitude = response.Longitude,
                event_date = response.EventDate,
                distance = response.Distance,
                event_type = response.EventType,
                status = response.Status
            });
        }

        [HttpGet]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetEvents()
        {
            EventService.EventListResponse response = _eventService.GetEvents();
            return Ok(response.Events);
        }

        [HttpGet("{eventId}")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetEvent(int eventId)
        {
            EventService.EventDetailResponse response = _eventService.GetEvent(eventId);

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(new
            {
                event_id = response.EventId,
                event_name = response.EventName,
                description = response.Description,
                location = response.Location,
                latitude = response.Latitude,
                longitude = response.Longitude,
                event_date = response.EventDate,
                distance = response.Distance,
                event_type = response.EventType,
                status = response.Status
            });
        }

        [HttpPut("{eventId}")]
        [RequireRole("organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateEvent(int eventId, [FromBody] EventRequest request)
        {
            EventService.EventUpdateResponse response = _eventService.UpdateEvent(eventId, request);

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

            return Ok(new
            {
                event_id = response.EventId,
                event_name = response.EventName,
                description = response.Description,
                location = response.Location,
                latitude = response.Latitude,
                longitude = response.Longitude,
                event_date = response.EventDate,
                distance = response.Distance,
                event_type = response.EventType,
                status = response.Status
            });
        }

        [HttpDelete("{eventId}")]
        [RequireRole("organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteEvent(int eventId)
        {
            EventService.EventDeleteResponse response = _eventService.DeleteEvent(eventId);

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return NoContent();
        }
    }
}
