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
}
