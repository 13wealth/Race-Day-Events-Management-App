
using System.ComponentModel.DataAnnotations;                                            // Gives us access to validation attributes such as [Required] and [EmailAddress]

namespace RaceDay.MVC.ViewModels;

public class LoginVm
{
    [Required, EmailAddress]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}