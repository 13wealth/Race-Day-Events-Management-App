using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    public class EnrollmentRequest
    {
        [Required]
        public int CategoryId { get; set; }

        public int EventId { get; set; }
    }

    public class EnrollmentResponse
    {
        public int EnrollmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class EnrollmentCreateResponse
    {
        public int EnrollmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class EnrollmentListResponse
    {
        public List<EnrollmentResponse> Enrollments { get; set; } = [];
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class EnrollmentEventListResponse
    {
        public List<EnrollmentResponse> Enrollments { get; set; } = [];
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}
