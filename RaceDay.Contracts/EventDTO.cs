using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    /// <summary>
    /// Data Transfer Object for creating and updating race events.
    /// </summary>
    public class EventRequest
    {
        [Required]
        public string EventName { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        public string Location { get; set; } = "";

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = "";

        [Required]
        public string Status { get; set; } = "";
    }

    public class EventResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public string Location { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class EventCreateResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public string Location { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class EventUpdateResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public string Location { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class EventDetailResponse
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public string Location { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class EventListResponse
    {
        public List<EventResponse> Events { get; set; } = [];
    }

    public class EventDeleteResponse
    {
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}
