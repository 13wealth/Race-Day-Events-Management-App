using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Contracts
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public string Phone { get; set; } = "";
    }

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

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = "";
    }

    public class AuthErrorResponse
    {
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}