using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTO
{
    /*
     * Data Transfer Object for creating and updating race events.
     */
    public class EventRequest
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
