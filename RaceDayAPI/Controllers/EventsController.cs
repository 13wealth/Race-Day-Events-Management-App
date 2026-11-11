using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;
using Microsoft.AspNetCore.Http;
using RaceDay.API.Storage;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages race events.
    /// Provides endpoints for creating, retrieving, updating, and deleting events.
    /// </summary>
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly IBlobStorageService _blobStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsController"/> class.
        /// </summary>
        /// <param name="eventService">The event service.</param>
        /// <param name="blobStorageService">The blob storage service.</param>
        public EventsController(
                                EventService eventService, 
                                IBlobStorageService blobStorageService
            )
        {
            _eventService = eventService;
            _blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Creates a new race event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="request">The event request containing event details.</param>
        /// <returns>
        /// A 201 Created response with the event details on success,
        /// a 400 Bad Request if validation fails,
        /// a 403 Forbidden if user is not an organizer,
        /// or a 404 Not Found if a required resource doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Retrieves all race events.
        /// </summary>
        /// <returns>A 200 OK response with the list of all events.</returns>
        [HttpGet]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetEvents()
        {
            EventService.EventListResponse response = _eventService.GetEvents();
            return Ok(response.Events);
        }

        /// <summary>
        /// Retrieves all race events belonging to the currently logged-in organiser.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the list of events owned by the organiser,
        /// a 403 Forbidden if user is not an organizer,
        /// or a 404 Not Found if organiser profile doesn't exist.
        /// </returns>
        [HttpGet("mine")]
        [RequireRole("organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetMyEvents()
        {
            EventService.EventListResponse response = _eventService.GetUserEvents();

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            return Ok(response.Events);
        }


        /// <summary>
        /// Retrieves a specific event by ID.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>
        /// A 200 OK response with the event details,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
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
                status = response.Status,
                banner_image_url = response.BannerImageUrl
            });
        }


        /// <summary>
        /// Updates an existing event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event to update.</param>
        /// <param name="request">The event request containing updated details.</param>
        /// <returns>
        /// A 200 OK response with the updated event details on success,
        /// a 400 Bad Request if validation fails,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
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


        /// <summary>
        /// Deletes a race event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event to delete.</param>
        /// <returns>
        /// A 204 No Content response on successful deletion,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Uploads a banner image for an event.
        /// Only the organiser who owns the event can perform this action.
        /// </summary>
        [HttpPost("{eventId}/banner")]
        [RequireRole("organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UploadEventBanner(
            int eventId, IFormFile file)
        {
            try
            {
                EventService.EventBannerResponse response =
                    await _eventService.UploadEventBannerAsync(eventId, file);

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
                    banner_image_url = response.BannerImageUrl
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        /// <summary>
        /// Retrieves the banner image for a specific event.    
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet("{eventId}/banner")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetEventBanner(int eventId)
        {
            EventService.EventDetailResponse response =
                _eventService.GetEvent(eventId);

            if (response.StatusCode == 404)
                return NotFound(new { error = response.Error });

            if (string.IsNullOrWhiteSpace(response.BannerImageUrl))
                return NotFound(new { error = "Event banner not found." });

            var image = await _blobStorageService.DownloadProfileImageAsync(
                response.BannerImageUrl);

            if (image.Content == null)
                return NotFound(new { error = "Event banner image not found." });

            return File(
                image.Content,
                image.ContentType ?? "application/octet-stream");
        }

    }
}
