
/***************************************************************************************
 *                                  REGISTER VIEW MODEL                                *
 * Holds the information entered by a user when registering for the RaceDay application.
 * *************************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class RegisterVm
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Participant";

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;
}