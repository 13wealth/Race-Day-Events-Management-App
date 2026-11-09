using System.Text.Json.Serialization;

namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantEnrollmentApiResponse
    {
        [JsonPropertyName("enrolment_id")]
        public int EnrollmentId { get; set; }

        [JsonPropertyName("participant_id")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("entry_date")]
        public DateTime EntryDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }

    public class ParticipantEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class ParticipantMyEventsViewModel
    {
        public List<ParticipantEnrollmentViewModel> Enrollments { get; set; }
            = new List<ParticipantEnrollmentViewModel>();
    }

    public class ParticipantCategoryApiResponse
    {
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = "";
    }
}