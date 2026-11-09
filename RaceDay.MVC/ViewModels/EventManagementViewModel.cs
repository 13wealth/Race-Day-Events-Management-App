/***************************************************************************************
 *                           EVENT MANAGEMENT VIEW MODEL                               *
 * Holds the data entered by an organiser when creating or editing an event.           *
 * It also provides validation rules before information is submitted to the API.       *
 ***************************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class EventManagementViewModel
    {
        [Required(ErrorMessage = "Please enter an event name.")]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = "";

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter the event location.")]
        public string Location { get; set; } = "";

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Please select an event date.")]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Range(0.01, 1000,
            ErrorMessage = "Distance must be greater than zero.")]
        public decimal Distance { get; set; }

        [Required(ErrorMessage = "Please select an event type.")]
        [RegularExpression("^(run|walk|cycle)$",
            ErrorMessage = "Event type must be run, walk or cycle.")]
        [Display(Name = "Event Type")]
        public string EventType { get; set; } = "run";

        [Required]
        public string Status { get; set; } = "scheduled";
    }
}