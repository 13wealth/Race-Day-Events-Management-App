using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

/// <summary>
/// Represents a role in the RaceDay application.
/// Roles define the permissions and responsibilities of users.
/// </summary>
[Table("ROLE")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required, StringLength(50)]
    [Column("role_name", TypeName = "varchar(50)")]
    public string RoleName { get; set; } = string.Empty;

    [Column("description", TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }
}
