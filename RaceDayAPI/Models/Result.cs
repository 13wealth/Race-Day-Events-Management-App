using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Models;

/// <summary>
/// Represents the result of a participant's race,
/// which includes their start time, finish time, and position.
/// </summary>
[Table("RESULTS")]
[Index(nameof(EnrollmentId), IsUnique = true)]
public class RaceResult
{
    [Key]
    [Column("results_id")]
    public int ResultsId { get; set; }

    [Column("enrolment_id")]
    public int EnrollmentId { get; set; }

    [Column("start_time", TypeName = "datetime2")]
    public DateTime StartTime { get; set; }

    [Column("finish_time", TypeName = "datetime2")]
    public DateTime FinishTime { get; set; }

    [Column("position")]
    public int Position { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime? UpdatedAt { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
}
