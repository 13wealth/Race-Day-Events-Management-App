using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            AuthService.RegisterResponse response = await _authService.RegisterAsync(request);

            if (response.StatusCode == 400)
            {
                return BadRequest(new { error = response.Error });
            }

            if (response.StatusCode == 409)
            {
                return Conflict(new { error = response.Error });
            }

            return StatusCode(201, new
            {
                user_id = response.UserId,
                username = response.Username,
                email = response.Email,
                role = response.Role,
                created_at = response.CreatedAt
            });
        }

        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            AuthService.LoginResponse response = await _authService.LoginAsync(request);

            if (response.StatusCode == 401)
            {
                return Unauthorized(new { error = response.Error });
            }

            return Ok(new
            {
                user_id = response.UserId,
                username = response.Username,
                email = response.Email,
                role = response.Role
            });
        }

        [HttpPost("logout")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Logout()
        {
            _authService.Logout();
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("me")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Me()
        {
            AuthService.CurrentUserResponse response = _authService.GetCurrentUser();
            return Ok(new
            {
                user_id = response.UserId,
                role = response.Role
            });
        }
    }
}