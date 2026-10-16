using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.DTO;

public class RegisterRequest
{
    [Required] public string Username { get; set; } = string.Empty;                                 // Username is required, defaults to an empty string
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;                      // Email is required and must be a valid email address, defaults to an empty string
    [Required] public string Password { get; set; } = string.Empty;                                 // Password is required, defaults to an empty string
    [Required] public string Role { get; set; } = string.Empty;                                     // Role is required, defaults to an empty string
    [Required] public string FirstName { get; set; } = string.Empty;                                // FirstName is required, defaults to an empty string
    [Required] public string LastName { get; set; } = string.Empty;                                 // LastName is required, defaults to an empty string
    public string? Phone { get; set; }                                                              // Phone is optional, can be null
}

/*
 * The LoginRequest class represents the data required for a user to log in.
 * It contains two properties: Username and Password, both of which are required.
 */
public class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}