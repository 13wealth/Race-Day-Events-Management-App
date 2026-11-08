using Microsoft.AspNetCore.Mvc;                                                         // Enables the use of ASP.NET Core MVC features such as controllers and routing
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Handles user authentication operations including registration, login, and logout.
    /// </summary>
    [ApiController]
    [Route("api/user")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration request containing username, email, and password.</param>
        /// <returns>
        /// A 201 Created response with the user details on success,
        /// a 400 Bad Request if validation fails,
        /// or a 409 Conflict if the user already exists.
        /// </returns>
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

        /// <summary>
        /// Authenticates a user with their credentials.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>
        /// A 200 OK response with user details on successful authentication,
        /// or a 401 Unauthorized if credentials are invalid.
        /// </returns>
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

        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A 200 OK response on successful logout,
        /// or a 401 Unauthorized if user is not authenticated.
        /// </returns>
        [HttpPost("logout")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Logout()
        {
            _authService.Logout();
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the current user's details,
        /// or a 401 Unauthorized if user is not authenticated.
        /// </returns>
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