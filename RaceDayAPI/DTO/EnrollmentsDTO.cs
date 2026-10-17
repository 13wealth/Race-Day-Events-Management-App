using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTO
{
    public class EnrollmentRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
