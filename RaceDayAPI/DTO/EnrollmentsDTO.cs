using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTO
{
    public class EnrollmentRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
