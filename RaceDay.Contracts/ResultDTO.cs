using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    public class ResultRequest
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime FinishTime { get; set; }

        [Required]
        public int Position { get; set; }
    }

    public class ResultResponse
    {
        public int ResultsId { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Position { get; set; }
    }

    public class ResultCreateResponse
    {
        public int ResultsId { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Position { get; set; }
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class ResultDetailResponse
    {
        public int ResultsId { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Position { get; set; }
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}
