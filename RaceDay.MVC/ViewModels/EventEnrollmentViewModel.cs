/***************************************************************************************
 *                         EVENT ENROLLMENT VIEW MODEL                                 *
 * Holds enrollment information for a specific event.                                  *
 * It includes:                                                                        *
 *  1. Event information – identifies the event being managed.                         *
 *  2. Enrollment details – stores participant and category information.               *
 *  3. Enrollment status – indicates the participant's enrollment status.              *
 ***************************************************************************************/

using System.Text.Json.Serialization;

namespace RaceDay.Mvc.ViewModels
{
    public class EventEnrollmentViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        public List<EnrollmentApiResponse> Enrollments { get; set; }
            = new List<EnrollmentApiResponse>();
    }

    /***************************************************************************************
     *                         ENROLLMENT API RESPONSE                                     *
     * Represents an enrollment returned by the RaceDay API.                               *
     * JSON attributes map the API's snake_case fields to C# properties.                  *
     ***************************************************************************************/
    public class EnrollmentApiResponse
    {
        [JsonPropertyName("enrolment_id")]
        public int EnrollmentId { get; set; }

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("participant_name")]
        public string ParticipantName { get; set; } = "";

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = "";

        [JsonPropertyName("entry_date")]
        public DateTime EntryDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }
}