using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Models;

/// <summary>
/// Represents a user in the RaceDay application,
/// which can be either a participant or an organiser.
/// </summary>
[Table("USER")]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class RaceDayUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required, StringLength(100)]
    [Column("username", TypeName = "nvarchar(100)")]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Column("password_hash", TypeName = "nvarchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Column("email", TypeName = "nvarchar(150)")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Column("role", TypeName = "varchar(20)")]
    public string Role { get; set; } = "participant";

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    public Profile? Profile { get; set; }
}

/// <summary>
/// Stores additional information about a user,
/// such as their first name, last name, and phone number.
/// </summary>
[Table("PROFILE")]
[Index(nameof(UserId), IsUnique = true)]
public class Profile
{
    [Key]
    [Column("profile_id")]
    public int ProfileId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    public RaceDayUser User { get; set; } = null!;
    public Organiser? Organiser { get; set; }
    public Participant? Participant { get; set; }
}

/// <summary>
/// Represents an organiser in the RaceDay application,
/// which is a user who can create and manage race events.
/// </summary>
[Table("ORGANISER")]
[Index(nameof(ProfileId), IsUnique = true)]
public class Organiser
{
    [Key]
    [Column("organiser_id")]
    public int OrganiserId { get; set; }

    [Column("profile_id")]
    public int ProfileId { get; set; }

    [Required, StringLength(100)]
    [Column("first_name", TypeName = "varchar(100)")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("last_name", TypeName = "varchar(100)")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("phone", TypeName = "varchar(20)")]
    public string? Phone { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime? UpdatedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public ICollection<RaceEvent> Events { get; set; } = new List<RaceEvent>();
}

/// <summary>
/// Represents a participant in the RaceDay application,
/// which is a user who can register for and participate in race events.
/// </summary>
[Table("PARTICIPANT")]
[Index(nameof(ProfileId), IsUnique = true)]
public class Participant
{
    [Key]
    [Column("participant_id")]
    public int ParticipantId { get; set; }

    [Column("profile_id")]
    public int ProfileId { get; set; }

    [Required, StringLength(100)]
    [Column("first_name", TypeName = "varchar(100)")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("last_name", TypeName = "varchar(100)")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("phone", TypeName = "varchar(20)")]
    public string? Phone { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime? UpdatedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
