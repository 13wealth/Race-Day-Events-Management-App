using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTO;
using RaceDay.API.Models;
using EventModel = RaceDay.API.Models.RaceEvent;

namespace RaceDay.API.Services
{
    public class EventService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;

        public EventService(RaceDayContext db, AuthenticationHelper authHelper, ValidationHelper validationHelper)
        {
            _db = db;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
        }

        public EventCreateResponse CreateEvent(EventRequest request)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new EventCreateResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            string eventTypeError = ValidationHelper.ValidateEventType(request.EventType);
            if (eventTypeError != null)
            {
                return new EventCreateResponse { Error = eventTypeError, StatusCode = 400 };
            }

            string statusError = ValidationHelper.ValidateEventStatus(request.Status);
            if (statusError != null)
            {
                return new EventCreateResponse { Error = statusError, StatusCode = 400 };
            }

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

            _db.Events.Add(newEvent);
            _db.SaveChanges();

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

        public EventListResponse GetEvents()
        {
            List<EventModel> events = _db.Events.ToList();

            EventListResponse response = new EventListResponse();
            response.Events = new List<object>();

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

        public EventDetailResponse GetEvent(int eventId)
        {
            EventModel found = _db.Events.FirstOrDefault(e => e.EventId == eventId);

            if (found == null)
            {
                return new EventDetailResponse { Error = "Event not found.", StatusCode = 404 };
            }

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

        public EventUpdateResponse UpdateEvent(int eventId, EventRequest request)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new EventUpdateResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            EventModel found = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (found == null)
            {
                return new EventUpdateResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                return new EventUpdateResponse { Error = "You can only change your own events.", StatusCode = 403 };
            }

            string eventTypeError = ValidationHelper.ValidateEventType(request.EventType);
            if (eventTypeError != null)
            {
                return new EventUpdateResponse { Error = eventTypeError, StatusCode = 400 };
            }

            string statusError = ValidationHelper.ValidateEventStatus(request.Status);
            if (statusError != null)
            {
                return new EventUpdateResponse { Error = statusError, StatusCode = 400 };
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

        public EventDeleteResponse DeleteEvent(int eventId)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new EventDeleteResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            EventModel found = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (found == null)
            {
                return new EventDeleteResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                return new EventDeleteResponse { Error = "You can only delete your own events.", StatusCode = 403 };
            }

            _db.Events.Remove(found);
            _db.SaveChanges();

            EventDeleteResponse response = new EventDeleteResponse();
            response.StatusCode = 204;
            return response;
        }
    }

    public class EventCreateResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public double Distance { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class EventListResponse
    {
        public List<object> Events { get; set; }
        public int StatusCode { get; set; }
    }

    public class EventDetailResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public double Distance { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class EventUpdateResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public double Distance { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class EventDeleteResponse
    {
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }
}
