using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTO;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using static System.Collections.Specialized.BitVector32;
using UserModel = RaceDayAPI.Models.RaceDayUser;

namespace RaceDayAPI.Controllers
{

    [ApiController]
    [Route("api/user")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _db;
        private readonly IPasswordHasher<UserModel> _passwordHasher;

        public AuthController(RaceDayContext db, IPasswordHasher<UserModel> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }


        /// <summary>
        /// Registers a user as an organiser or participant, with personal details. 
        /// Returns 201, or 409 if the username or email is taken.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string role = request.Role.ToLower();

            if (role != "organiser" && role != "participant")
            {
                return BadRequest(new { error = "Role must be 'organiser' or 'participant'." });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new { error = "Password must be at least 8 characters." });
            }

            /*
             * Check if the username or email is already taken
             */
            var existing = await (from u in _db.Users
                                  where u.Username == request.Username || u.Email == request.Email
                                  select u).FirstOrDefaultAsync();

            if (existing != null)
            {
                return Conflict(new { error = "Username or email already exists." });
            }

            DateTime now = DateTime.UtcNow;

            /*
             * Create the user and hash the password (never store the plain password)
             */
            UserModel user = new UserModel();
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = role;
            user.CreatedAt = now;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            /*
             * Create the profile that links the user to a role record
             */
            Profile profile = new Profile();
            profile.User = user;
            profile.CreatedAt = now;
            user.Profile = profile;

            
            if (role == "organiser")
            {
                Organiser organiser = new Organiser();
                organiser.FirstName = request.FirstName;
                organiser.LastName = request.LastName;
                organiser.Phone = request.Phone;
                organiser.CreatedAt = now;
                profile.Organiser = organiser;
            }
            else
            {
                Participant participant = new Participant();
                participant.FirstName = request.FirstName;
                participant.LastName = request.LastName;
                participant.Phone = request.Phone;
                participant.CreatedAt = now;
                profile.Participant = participant;
            }

            /*
             * One save inserts the USER, PROFILE and role record
             */
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                user_id = user.UserId,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                created_at = user.CreatedAt
            });
        }

        /// <summary>
        /// Logs in with username and password and starts a session.
        /// Returns 200, or 401 if the details are wrong.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await (from u in _db.Users
                              where u.Username == request.Username
                              select u).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { error = "Invalid username or password." });
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { error = "Invalid username or password." });
            }

            /*
             * Store the logged-in user's id and role in the session
             */
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new
            {
                user_id = user.UserId,
                username = user.Username,
                email = user.Email,
                role = user.Role
            });
        }

        /// <summary>
        /// Ends the current session.
        /// Requires a logged-in user.
        /// </summary>
        [HttpPost("logout")]
        [RequireRole]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Returns the user id and role stored in the current session.
        /// Requires a logged-in user.
        /// </summary>
        [HttpGet("me")]
        [RequireRole]
        public IActionResult Me()
        {
            return Ok(new
            {
                user_id = HttpContext.Session.GetInt32("UserId"),
                role = HttpContext.Session.GetString("Role")
            });
        }
    }
}