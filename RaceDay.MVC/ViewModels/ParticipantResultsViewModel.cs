using System.Text.Json.Serialization;

namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantResultApiResponse
    {
        [JsonPropertyName("results_id")]
        public int ResultsId { get; set; }

        [JsonPropertyName("enrolment_id")]
        public int EnrollmentId { get; set; }

        [JsonPropertyName("event_name")]
        public string EventName { get; set; }

        [JsonPropertyName("event_date")]
        public DateTime EventDate { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("finish_time")]
        public DateTime FinishTime { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }
    }

    public class ParticipantResultsViewModel
    {
        public List<ParticipantResultApiResponse> Results { get; set; }
            = new List<ParticipantResultApiResponse>();
    }
}