using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    public class CategoryRequest
    {
        [Required]
        public string CategoryName { get; set; } = "";
    }
}