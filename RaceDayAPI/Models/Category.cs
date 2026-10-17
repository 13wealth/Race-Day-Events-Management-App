using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Models;

/// <summary>
/// Represents a category within a race event,
/// which participants can register for.
/// </summary>
[Table("CATEGORY")]
[Index(nameof(EventId), nameof(CategoryName), IsUnique = true)]
public class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Required, StringLength(100)]
    [Column("category_name", TypeName = "varchar(100)")]
    public string CategoryName { get; set; } = string.Empty;

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    public RaceEvent Event { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
