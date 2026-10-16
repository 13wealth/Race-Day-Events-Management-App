using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTO
{
    public class CategoryRequest
    {
        [Required]
        public string CategoryName { get; set; } = "";
    }
}