
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RaceDayAPI.Models;

/*
 * Represents a user in the RaceDay application, 
 *  which can be either a participant or an organiser.
 */
[Table("USER")]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class RaceDayUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }                                                     // Represents a user in the RaceDay application

    [Required, StringLength(100)]
    [Column("username", TypeName = "nvarchar(100)")]
    public string Username { get; set; } = string.Empty;                                // Represents the username of the user, which is unique and required

    [Required, StringLength(255)]
    [Column("password_hash", TypeName = "nvarchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;                            // Represents the hashed password of the user, which is required

    [Required, StringLength(150)]
    [Column("email", TypeName = "nvarchar(150)")]
    public string Email { get; set; } = string.Empty;                                   // Represents the email address of the user, which is unique and required

    [Required, StringLength(20)]
    [Column("role", TypeName = "varchar(20)")]
    public string Role { get; set; } = "participant";                                   // Represents the role of the user, which can be either "participant" or "organiser", with a default value of "participant"

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }                                             // Represents the date and time when the user was created

    public Profile? Profile { get; set; }
}

/*
 * Stores additional information about a user, 
 *  such as their first name, last name, and phone number.
 */
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

/*
 * Represents an organiser in the RaceDay application, 
 *  which is a user who can create and manage race events.
 */
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

/*
 * Represents a participant in the RaceDay application, 
 *  which is a user who can register for and participate in race events.
 */
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

/*
 * Represents a race event in the RaceDay application, 
 *  which is created and managed by an organiser.
 */
[Table("EVENT")]
public class RaceEvent
{
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("organiser_id")]
    public int OrganiserId { get; set; }

    [Required, StringLength(200)]
    [Column("event_name", TypeName = "varchar(200)")]
    public string EventName { get; set; } = string.Empty;

    [Column("description", TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    [Required, StringLength(200)]
    [Column("location", TypeName = "varchar(200)")]
    public string Location { get; set; } = string.Empty;

    [Column("latitude", TypeName = "decimal(10,8)")]
    public decimal Latitude { get; set; }

    [Column("longitude", TypeName = "decimal(11,8)")]
    public decimal Longitude { get; set; }

    [Column("event_date", TypeName = "datetime2")]
    public DateTime EventDate { get; set; }

    [Column("distance", TypeName = "decimal(6,2)")]
    public decimal Distance { get; set; }

    [Required, StringLength(10)]
    [Column("event_type", TypeName = "varchar(10)")]
    public string EventType { get; set; } = "run";

    [Required, StringLength(20)]
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; } = "scheduled";

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime? UpdatedAt { get; set; }

    public Organiser Organiser { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

/*
 * Represents a category within a race event, 
 *  which participants can register for.
 */
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

/*
 * Represents a route within a race event, 
 *  which participants will follow during the race.
 */
[Table("ROUTE")]
public class Route
{
    [Key]
    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Required, StringLength(200)]
    [Column("route_name", TypeName = "nvarchar(200)")]
    public string RouteName { get; set; } = string.Empty;

    [Column("distance", TypeName = "decimal(10,2)")]
    public decimal Distance { get; set; }

    [Required, StringLength(200)]
    [Column("start_location", TypeName = "nvarchar(200)")]
    public string StartLocation { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [Column("end_location", TypeName = "nvarchar(200)")]
    public string EndLocation { get; set; } = string.Empty;

    [Column("map_url", TypeName = "nvarchar(max)")]
    public string? MapUrl { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime2")]
    public DateTime? UpdatedAt { get; set; }

    public RaceEvent Event { get; set; } = null!;
}

/*
 * Represents an enrollment of a participant in a race event, 
 *  which includes the category they registered for and their race result.
 */
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

/*
 * Represents the result of a participant's race, 
 *  which includes their start time, finish time, and position.
 */
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
