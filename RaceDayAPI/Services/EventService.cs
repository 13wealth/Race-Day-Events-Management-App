using RaceDay.API.Data;
using RaceDay.Contracts;
using RaceDay.API.Models;
using EventModel = RaceDay.API.Models.RaceEvent;
using RaceDay.API.Storage;

namespace RaceDay.API.Services
{
    public class EventService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;
        private readonly IBlobStorageService _blobStorageService;

        public EventService(
                            RaceDayContext db, 
                            AuthenticationHelper authHelper, 
                            ValidationHelper validationHelper, 
                            IBlobStorageService blobStorageService
            )
        {
            _db = db;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
            _blobStorageService = blobStorageService;
        }

        /*
         * Creates a new event record in the database for the logged-in organiser.
         * Verifies that the logged-in user is an organiser before allowing event creation.
         * Validates the event type and status using ValidationHelper methods.
         * Assigns the current organiser as the owner of the new event.
         * Returns an EventCreateResponse with created event details and 201 status code on success.
         * Returns error responses with appropriate status codes (404, 400, 403) for validation or authorization failures.
         */
        public EventCreateResponse CreateEvent(EventRequest request)
        {
            
            Organiser organiser = _authHelper.GetLoggedInOrganiser();                   // Get the logged-in organiser from the session and authentication helper
            
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                EventCreateResponse organiserErrorResponse = new EventCreateResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Validate the event type provided in the request
            string eventTypeError = ValidationHelper.ValidateEventType(request.EventType);
            if (eventTypeError != null)                                                 // Check if event type validation failed
            {
                EventCreateResponse typeErrorResponse = new EventCreateResponse();
                typeErrorResponse.Error = eventTypeError;
                typeErrorResponse.StatusCode = 400;
                return typeErrorResponse;
            }

            // Validate the event status provided in the request
            string statusError = ValidationHelper.ValidateEventStatus(request.Status);
            if (statusError != null)                                                    // Check if status validation failed
            {
                EventCreateResponse statusErrorResponse = new EventCreateResponse();
                statusErrorResponse.Error = statusError;
                statusErrorResponse.StatusCode = 400;
                return statusErrorResponse;
            }

            // Create a new event object with the request details and assign it to the organiser
            EventModel newEvent = new EventModel();
            newEvent.OrganiserId = organiser.OrganiserId;
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

            // Add the new event to the database and save changes
            _db.Events.Add(newEvent);
            _db.SaveChanges();

            // Create success response with the created event details
            EventCreateResponse response = new EventCreateResponse();
            response.EventId = newEvent.EventId;
            response.EventName = newEvent.EventName;
            response.Description = newEvent.Description;
            response.Location = newEvent.Location;
            response.Latitude = newEvent.Latitude;
            response.Longitude = newEvent.Longitude;
            response.EventDate = newEvent.EventDate;
            response.Distance = newEvent.Distance;
            response.EventType = newEvent.EventType;
            response.Status = newEvent.Status;
            response.StatusCode = 201;

            return response;
        }

        /*
         * Retrieves all events from the database.
         * Returns a list of event objects with basic event information.
         * Converts each event to an anonymous object format for API response.
         * Returns an EventListResponse with 200 status code on success.
         */
        public EventListResponse GetEvents()
        {
            // Retrieve all events from the database and add them to a list
            List<EventModel> events = new List<EventModel>();
            foreach (EventModel e in _db.Events)
            {
                events.Add(e);
            }

            // Create response object and initialize the events list
            EventListResponse response = new EventListResponse();
            response.Events = new List<object>();

            /*
             * Iterate through all events and convert each to an anonymous object format.
             * This format is used for the API response to format field names appropriately.
             */
            foreach (EventModel e in events)
            {
                object eventObj = new
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
                response.Events.Add(eventObj);
            }

            response.StatusCode = 200;
            return response;
        }

        /*
         * Retrieves all events belonging to the logged-in organiser.
         * Filters events by matching OrganiserId to the current organiser.
         * Returns a list of event objects belonging only to this organiser.
         * Returns an EventListResponse with 200 status code on success.
         * Returns error response with 404 status code if organiser profile not found.
         */
        public EventListResponse GetUserEvents()
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                EventListResponse organiserErrorResponse = new EventListResponse();
                organiserErrorResponse.Events = new List<object>();
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Retrieve only events belonging to the logged-in organiser
            List<EventModel> userEvents = new List<EventModel>();
            foreach (EventModel e in _db.Events)
            {
                if (e.OrganiserId == organiser.OrganiserId)                            // Check if event belongs to this organiser
                {
                    userEvents.Add(e);
                }
            }

            // Create response object and initialize the events list
            EventListResponse response = new EventListResponse();
            response.Events = new List<object>();

            /*
             * Iterate through the user's events and convert each to an anonymous object format.
             * This format is used for the API response to format field names appropriately.
             */
            foreach (EventModel e in userEvents)
            {
                object eventObj = new
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
                response.Events.Add(eventObj);
            }

            response.StatusCode = 200;
            return response;
        }

        /*
         * Retrieves a specific event by its event ID.
         * Searches the database for the event matching the provided ID.
         * Returns an EventDetailResponse with event details and 200 status code on success.
         * Returns error response with 404 status code if event is not found.
         */
        public EventDetailResponse GetEvent(int eventId)
        {
            // Search for an event with the matching event ID in the database
            EventModel? found = null;                                                    // Initialize found event to null
            foreach (EventModel e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    found = e;                                                          // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no event was found with that ID
            {
                EventDetailResponse notFoundResponse = new EventDetailResponse();
                notFoundResponse.Error = "Event not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            // Create success response with the event details
            EventDetailResponse response = new EventDetailResponse();
            response.EventId = found.EventId;
            response.EventName = found.EventName;
            response.Description = found.Description;
            response.Location = found.Location;
            response.Latitude = found.Latitude;
            response.Longitude = found.Longitude;
            response.EventDate = found.EventDate;
            response.Distance = found.Distance;
            response.EventType = found.EventType;
            response.Status = found.Status;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Updates an existing event with new information.
         * Verifies that the logged-in user is an organiser and owns the event before allowing updates.
         * Validates the new event type and status using ValidationHelper methods.
         * Updates all event fields with the provided request data.
         * Returns an EventUpdateResponse with updated event details and 200 status code on success.
         * Returns error responses with appropriate status codes (404, 403, 400) for authorization or validation failures.
         */
        public EventUpdateResponse UpdateEvent(int eventId, EventRequest request)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                EventUpdateResponse organiserErrorResponse = new EventUpdateResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Search for the event to be updated by its event ID
            EventModel? found = null;                                                    // Initialize found event to null
            foreach (EventModel e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    found = e;                                                          // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no event was found with that ID
            {
                EventUpdateResponse notFoundResponse = new EventUpdateResponse();
                notFoundResponse.Error = "Event not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from updating other organisers' events.
             */
            if (found.OrganiserId != organiser.OrganiserId)                            // Check if organiser does not own this event
            {
                EventUpdateResponse forbiddenResponse = new EventUpdateResponse();
                forbiddenResponse.Error = "You can only change your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Validate the new event type provided in the request
            string eventTypeError = ValidationHelper.ValidateEventType(request.EventType);
            if (eventTypeError != null)                                                 // Check if event type validation failed
            {
                EventUpdateResponse typeErrorResponse = new EventUpdateResponse();
                typeErrorResponse.Error = eventTypeError;
                typeErrorResponse.StatusCode = 400;
                return typeErrorResponse;
            }

            // Validate the new event status provided in the request
            string statusError = ValidationHelper.ValidateEventStatus(request.Status);
            if (statusError != null)                                                    // Check if status validation failed
            {
                EventUpdateResponse statusErrorResponse = new EventUpdateResponse();
                statusErrorResponse.Error = statusError;
                statusErrorResponse.StatusCode = 400;
                return statusErrorResponse;
            }

            // Update all event fields with the new values from the request
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

            // Save changes to the database
            _db.SaveChanges();

            // Create success response with the updated event details
            EventUpdateResponse response = new EventUpdateResponse();
            response.EventId = found.EventId;
            response.EventName = found.EventName;
            response.Description = found.Description;
            response.Location = found.Location;
            response.Latitude = found.Latitude;
            response.Longitude = found.Longitude;
            response.EventDate = found.EventDate;
            response.Distance = found.Distance;
            response.EventType = found.EventType;
            response.Status = found.Status;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Deletes an event from the database.
         * Verifies that the logged-in user is an organiser and owns the event before allowing deletion.
         * Removes the event record from the database permanently.
         * Returns an EventDeleteResponse with 204 status code (No Content) on success.
         * Returns error responses with appropriate status codes (404, 403) for authorization or validation failures.
         */
        public EventDeleteResponse DeleteEvent(int eventId)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                EventDeleteResponse organiserErrorResponse = new EventDeleteResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Search for the event to be deleted by its event ID
            EventModel? found = null;                                                    // Initialize found event to null
            foreach (EventModel e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    found = e;                                                          // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no event was found with that ID
            {
                EventDeleteResponse notFoundResponse = new EventDeleteResponse();
                notFoundResponse.Error = "Event not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from deleting other organisers' events.
             */
            if (found.OrganiserId != organiser.OrganiserId)                            // Check if organiser does not own this event
            {
                EventDeleteResponse forbiddenResponse = new EventDeleteResponse();
                forbiddenResponse.Error = "You can only delete your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Remove the event from the database and save changes
            _db.Events.Remove(found);
            _db.SaveChanges();

            // Create response with 204 No Content status code
            EventDeleteResponse response = new EventDeleteResponse();
            response.StatusCode = 204;
            return response;
        }

        /*
         * Uploads a banner image for an existing event.
         * Verifies that the logged-in organiser owns the event.
         * Uploads the banner to Azure Blob Storage.
         * Saves the banner image URL against the event.
         * Returns appropriate status codes for success or errors.
         */
        public async Task<EventBannerResponse> UploadEventBannerAsync(
            int eventId, IFormFile file)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();

            if (organiser == null)
            {
                return new EventBannerResponse
                {
                    Error = "Organiser profile not found.",
                    StatusCode = 404
                };
            }

            EventModel? found = null;

            foreach (EventModel e in _db.Events)
            {
                if (e.EventId == eventId)
                {
                    found = e;
                    break;
                }
            }

            if (found == null)
            {
                return new EventBannerResponse
                {
                    Error = "Event not found.",
                    StatusCode = 404
                };
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                return new EventBannerResponse
                {
                    Error = "You can only upload banners for your own events.",
                    StatusCode = 403
                };
            }

            string imageUrl = await _blobStorageService.UploadEventBannerAsync(
                file, eventId);

            found.BannerImageUrl = imageUrl;
            found.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            return new EventBannerResponse
            {
                EventId = found.EventId,
                BannerImageUrl = found.BannerImageUrl,
                StatusCode = 200
            };
        }


        public class EventCreateResponse
        {
            public int EventId { get; set; }
            public string? EventName { get; set; }
            public string? Description { get; set; }
            public string? Location { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public DateTime EventDate { get; set; }
            public decimal Distance { get; set; }
            public string? EventType { get; set; }
            public string? Status { get; set; }
            public string? Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EventListResponse
        {
            public List<object>? Events { get; set; }
            public int StatusCode { get; set; }
        }

        public class EventDetailResponse
        {
            public int EventId { get; set; }
            public string? EventName { get; set; }
            public string? Description { get; set; }
            public string? Location { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public DateTime EventDate { get; set; }
            public decimal Distance { get; set; }
            public string? EventType { get; set; }
            public string? Status { get; set; }
            public string? Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EventUpdateResponse
        {
            public int EventId { get; set; }
            public string? EventName { get; set; }
            public string? Description { get; set; }
            public string? Location { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public DateTime EventDate { get; set; }
            public decimal Distance { get; set; }
            public string? EventType { get; set; }
            public string? Status { get; set; }
            public string? Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EventDeleteResponse
        {
            public string? Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EventBannerResponse
        {
            public int EventId { get; set; }
            public string? BannerImageUrl { get; set; }
            public string? Error { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
