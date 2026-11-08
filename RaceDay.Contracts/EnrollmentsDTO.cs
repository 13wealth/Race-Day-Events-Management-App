using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    public class EnrollmentRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
