/***************************************************************************************
 *                         ORGANISER DASHBOARD VIEW MODEL                              *
 * Carries the information required to display the organiser dashboard.                *
 * It contains:                                                                        *
 *  1. Dashboard totals – total events and total enrolments.                           *
 *  2. Upcoming events – events scheduled for today or a future date.                  *
 *  3. Event summaries – individual event details and enrolment counts.                *
 ***************************************************************************************/

using System.Text.Json.Serialization;

namespace RaceDay.Mvc.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        /*
         * List of events associated with the organiser.
         * Each event is represented by an OrganiserEventViewModel.
         */
        public List<OrganiserEventViewModel> Events { get; set; } = new();

        /*
         * Read-only properties 
         * Calculates totals and counts based on the Events list.
         */
        public int TotalEvents
        {
            get { return Events.Count; }
        }


        /*
         * Calculates the total number of enrolments across all events.
         * If an event's enrolment count is null, it is treated as zero.
            .Sum() – adds up the values.
             e – represents one event at a time.
             => – lambda operator, meaning "goes to".
             e.EnrolmentCount – gets the enrolment count for that event.
             ?? 0 – if the value is null, use 0.
         */
        public int TotalEnrolments
        {
            get
            {
                return Events.Sum(e => e.EnrolmentCount ?? 0);
            }
        }


        /*
         * Calculates the number of upcoming events.
         * An upcoming event is defined as one that is scheduled for today or a future date.
         * Uses LINQ's Count() method to filter and count the events.
         */
        public int UpcomingEvents
        {
            get
            {
                return Events.Count(e => e.EventDate.Date >= DateTime.Today);
            }
        }


        /*
         * Indicates whether enrolment data is available for the events.
         * If false, it means that the enrolment data could not be retrieved from the API.
         * This property can be used to display an error message or alternative content in the view.
         */
        public bool EnrolmentDataAvailable { get; set; } = true;


        /*
         * Holds an error message if enrolment data is not available.
         * This can be used to inform the user about issues with data retrieval.
         */
        public string? ErrorMessage { get; set; }
    }

    /*
     * Represents the details of an individual event for the organiser dashboard.
     * Contains properties for event information and enrolment count.
     */
    public class OrganiserEventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public string Location { get; set; } = "";
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string Status { get; set; } = "";

        public int? EnrolmentCount { get; set; }
    }


    /*
     * Represents the event data returned by GET api/events/mine.
     * JsonPropertyName maps the API's snake_case fields to C# properties.
     */
    public class EventApiResponse
    {
        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("event_name")]
        public string EventName { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = "";

        [JsonPropertyName("event_date")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("distance")]
        public decimal Distance { get; set; }

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        [JsonPropertyName("banner_image_url")]
        public string? BannerImageUrl { get; set; }
    }


    /*
     * Represents the minimum enrolment data needed by the dashboard.
     * The dashboard only needs the number of returned enrolment records.
     */
    public class EnrolmentApiResponse
    {
        [JsonPropertyName("enrolment_id")]
        public int EnrolmentId { get; set; }
    }
}