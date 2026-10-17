using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Models;

/// <summary>
/// Represents an enrollment of a participant in a race event,
/// which includes the category they registered for and their race result.
/// </summary>
[Table("ENROLLMENT")]
[Index(nameof(ParticipantId), nameof(EventId), IsUnique = true)]
public class Enrollment
{
    [Key]
    [Column("enrolment_id")]
    public int EnrollmentId { get; set; }

    [Column("participant_id")]
    public int ParticipantId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("entry_date", TypeName = "datetime2")]
    public DateTime EntryDate { get; set; }

    [Required, StringLength(20)]
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; } = "registered";

    public Participant Participant { get; set; } = null!;
    public RaceEvent Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public RaceResult? Result { get; set; }
}
