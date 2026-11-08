using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Contracts
{
    /*
     * Resposnible for handling user registration requests.
     */
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = "";                  // Add a default value to avoid null reference issues

        [Required]
        [EmailAddress]                                              // Add EmailAddress attribute to validate email format
        public string Email { get; set; } = "";                     // Add a default value to avoid null reference issues

        [Required]
        public string Password { get; set; } = "";                  // Add a default value to avoid null reference issues

        [Required]
        public string Role { get; set; } = "";                      // Add a default value to avoid null reference issues

        [Required]
        public string FirstName { get; set; } = "";                 //  Add a default value to avoid null reference issues

        [Required]
        public string LastName { get; set; } = "";                  // Add a default value to avoid null reference issues

        public string Phone { get; set; } = "";                     // Add a default value to avoid null reference issues
    }

    /*
     * Responsible for handling user login requests.
     */
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    public class AuthResponse
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = "";
    }
}