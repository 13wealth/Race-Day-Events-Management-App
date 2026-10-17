using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;                                                                 // Import where your DbContext is defined
using RaceDay.API.DTO;                                                                  // Import where your DTOs are defined
using RaceDay.API.Models;                                                               // Import where your UserModel, Profile, Organiser, and Participant models are defined
using RaceDay.API.Security;                                                             // Import where your RequireRoleAttribute is defined

namespace RaceDay.API.Controllers
{

    [ApiController]                                                                     // Attribute tells ASP.NET this class handles API requests
    [Route("api/user")]                                                                 // Attribute defines the base route for all actions in this controller
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _db;                                            // Delcare a private readonly field that allows the controller to interact with the database.
        private readonly IPasswordHasher<RaceDayUser> _passwordHasher;                  // Declare a private readonly field that allows the controller to hash and verify passwords.

        /*
         * The constructor takes the DbContext and PasswordHasher as parameters.
         * These are provided by dependency injection.
         */
        public AuthController(RaceDayContext db, IPasswordHasher<RaceDayUser> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }


        /// <summary>
        /// Registers a user as an organiser or participant, with personal details.
        /// Returns 201, or 409 if the username or email is taken.
        /// </summary>
        [HttpPost("register")]                                                          // Defines endpoint route "api/user/register"
        [ProducesResponseType(201)]                                                     // Response type for successful registration
        [ProducesResponseType(400)]                                                     // Response type for bad request (invalid input)
        [ProducesResponseType(409)]                                                     // Response type for conflict (username or email already exists)

        /*
         * The Register method handles user registration.
         * It validates the input, checks for existing users, hashes the password,
         * creates the user and associated profile and saves them to the database.
         */
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            string role = request.Role.ToLower();                                       // Convert the role to lowercase for case-insensitive comparison

            if (role != "organiser" && role != "participant")                           // Checks if the role is valid (either "organiser" or "participant")
            {
                return BadRequest(
                    new 
                    { 
                        error = "Role must be 'organiser' or 'participant'." 
                    }
                );
            }

            if (request.Password.Length < 8)                                            // Checks password length
            {
                return BadRequest(
                    new 
                    { 
                        error = "Password must be at least 8 characters." 
                    }
                );
            }

            /*
             * Check if the username or email is already taken
             * Queries the database for any existing user with the same username or email.
             * Query written in C# LINQ syntax, which is translated to SQL by Entity Framework Core.
             */
            var existing = await (from u in _db.Users
                                  where u.Username == request.Username || u.Email == request.Email
                                  select u).FirstOrDefaultAsync(); 

            if (existing != null)                                                       // Use existing to check if user already exists
            {
                return Conflict(
                    new 
                    { 
                        error = "Username or email already exists." 
                    }
                );
            }

            DateTime now = DateTime.UtcNow;                                             // Get the current UTC time for record creation timestamps

            /*
             * Create the user and hash the password or build a user object
             * EF Core will turn this object into a SQL INSERT statement
             */
            RaceDayUser user = new RaceDayUser();                                       // Create a blank user object
            user.Username = request.Username;                                           // Set the username from the request
            user.Email = request.Email;                                                 // Set the email from the request
            user.Role = role;                                                           // Set the role from the request
            user.CreatedAt = now;                                                       // Set the creation timestamp to the current UTC time
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);   // Hash the password and store it in the PasswordHash property

            /*
             * Create the profile that links the user to a role record
             * If auth is successful, we will create a profile for the user
             */
            Profile profile = new Profile();                                            // Create a blank profile object
            profile.User = user;                                                        // Link the profile to the user
            profile.CreatedAt = now;                                                    // Set the creation timestamp for the profile
            user.Profile = profile;                                                     // Link the user to the profile

            /*
             * Checks if role is organiser or participant
             * IF it is, creates the appropriate model object
             * Then links the profile to the role object
             */ 
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
             * Saves the user and profile to the database
             * Built in SaveChangesAsync() tell ASP.NET to generate INSERT statements
             */
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            /*
             * Bundles the user details into a response object
             * Returns 201 Created with the user details in the response body
             */ 
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
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
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
        /// Ends the current session. Requires a logged-in user.
        /// </summary>
        [HttpPost("logout")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
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
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
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