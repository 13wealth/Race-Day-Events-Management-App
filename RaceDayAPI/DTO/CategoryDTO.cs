using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTO
{
    public class CategoryRequest
    {
        [Required]
        public string CategoryName { get; set; } = "";
    }
}