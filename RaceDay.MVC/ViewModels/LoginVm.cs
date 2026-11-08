
using System.ComponentModel.DataAnnotations;                                            // Gives us access to validation attributes such as [Required] and [EmailAddress]

namespace RaceDay.Mvc.ViewModels;

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}