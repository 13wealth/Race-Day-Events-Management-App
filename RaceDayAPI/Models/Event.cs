using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

/// <summary>
/// Represents a race event in the RaceDay application,
/// which is created and managed by an organiser.
/// </summary>
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

/// <summary>
/// Represents a route within a race event,
/// which participants will follow during the race.
/// </summary>
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
