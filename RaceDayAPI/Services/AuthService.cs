using Microsoft.AspNetCore.Identity;
using RaceDay.API.Data;
using RaceDay.Contracts;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class AuthService
    {
        private readonly RaceDayContext _db;
        private readonly IPasswordHasher<RaceDayUser> _passwordHasher;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;

        public AuthService(RaceDayContext db, 
                           IPasswordHasher<RaceDayUser> passwordHasher, 
                           AuthenticationHelper authHelper, 
                           ValidationHelper validationHelper)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
        }

        /*
         * Registers a new user by creating a user account, profile, and role-specific entity (Organiser or Participant).
         * Validates the role and password before creating the user.
         * Hashes the password using the IPasswordHasher before saving to the database.
         * Returns a RegisterResponse with the created user details and a 201 status code on success.
         * Returns error responses with appropriate status codes (400, 409) for validation or duplicate user failures.
         */
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate the role provided in the request
            string roleError = ValidationHelper.ValidateRole(request.Role);

            if (roleError != null)                                                      // Check if role validation failed
            {
                RegisterResponse errorResponse = new RegisterResponse();                // Create error response object
                errorResponse.Error = roleError;
                errorResponse.StatusCode = 400;

               return errorResponse;
            }

            // Validate the password provided in the request
            string passwordError = ValidationHelper.ValidatePassword(request.Password);
            if (passwordError != null)                                                  // Check if password validation failed
            {
                RegisterResponse errorResponse = new RegisterResponse();                // Create error response object
                errorResponse.Error = passwordError;
                errorResponse.StatusCode = 400;
                return errorResponse;
            }

            // Convert role to lowercase for consistency
            string role = request.Role.ToLower();

            /*
             * Check if a user with the same username or email already exists.
             * Iterate through the database to find any duplicate.
             * If found, return a conflict error response with status code 409.
             */
            RaceDayUser existing = null;                                                // Initialize existing user to null
            foreach (RaceDayUser u in _db.Users)
            {
                if (u.Username == request.Username || u.Email == request.Email)        // Check for duplicate username or email
                {
                    existing = u;                                                       // Found a duplicate user
                    break;                                                              // Exit the loop immediately
                }
            }

            if (existing != null)                                                       // If a duplicate user already exists
            {
                RegisterResponse conflictResponse = new RegisterResponse();             // Create conflict response object
                conflictResponse.Error = "Username or email already exists.";
                conflictResponse.StatusCode = 409;
                return conflictResponse;
            }

            // Get the current UTC time for all timestamps
            DateTime now = DateTime.UtcNow;

            // Create a new user object with the registration details
            RaceDayUser user = new RaceDayUser();
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = role;
            user.CreatedAt = now;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);   // Hash the password using the password hasher

            // Create a new profile associated with the user
            Profile profile = new Profile();
            profile.User = user;
            profile.CreatedAt = now;
            user.Profile = profile;

            /*
             * Create a role-specific entity based on the user's role.
             * If role is "organiser", create an Organiser object.
             * Otherwise, create a Participant object for the "participant" role.
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

            // Save to database
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            RegisterResponse response = new RegisterResponse();
            response.UserId = user.UserId;
            response.Username = user.Username;
            response.Email = user.Email;
            response.Role = user.Role;
            response.CreatedAt = user.CreatedAt;
            response.StatusCode = 201;

            return response;
        }

        /*
         * Authenticates a user by verifying their username and password against stored credentials.
         * Retrieves the user from the database by username.
         * Verifies the provided password against the stored password hash using IPasswordHasher.
         * Sets the user session with user ID and role on successful authentication.
         * Returns a LoginResponse with user details and 200 status code on success.
         * Returns error responses with 401 status code for invalid username or password.
         */
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Search for a user with the matching username in the database
            RaceDayUser user = null;                                                    // Initialize user to null
            foreach (RaceDayUser u in _db.Users)
            {
                if (u.Username == request.Username)                                     // Check if username matches
                {
                    user = u;                                                           // Found the user
                    break;                                                              // Exit the loop immediately
                }
            }

            if (user == null)                                                           // If no user was found with that username
            {
                LoginResponse notFoundResponse = new LoginResponse();                   // Create error response object
                notFoundResponse.Error = "Invalid username or password.";
                notFoundResponse.StatusCode = 401;
                return notFoundResponse;
            }

            /*
             * Verify the provided password against the hashed password stored in the database.
             * Uses the IPasswordHasher VerifyHashedPassword method to safely compare passwords.
             * Returns PasswordVerificationResult.Failed if the password does not match.
             */
            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
                                                                                        user, 
                                                                                        user.PasswordHash, 
                                                                                        request.Password
                );
            if (result == PasswordVerificationResult.Failed)                            // Check if password verification failed
            {
                LoginResponse failedResponse = new LoginResponse();                     // Create error response object
                failedResponse.Error = "Invalid username or password.";
                failedResponse.StatusCode = 401;
                return failedResponse;
            }

            _authHelper.SetSession(user.UserId, user.Role);                             // Set the user session with user ID and role for future requests

            // Create success response with authenticated user details
            LoginResponse response = new LoginResponse();
            response.UserId = user.UserId;
            response.Username = user.Username;
            response.Email = user.Email;
            response.Role = user.Role;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Logs out the currently logged-in user.
         * Clears the user session data stored in the authentication helper.
         * No response body is returned for logout operations.
         */
        public void Logout()
        {
            _authHelper.ClearSession();                                                 // Remove all session information
        }

        /*
         * Retrieves information about the currently logged-in user.
         * Returns user's ID and role from the current session.
         * Returns a CurrentUserResponse with user information.
         */
        public CurrentUserResponse GetCurrentUser()
        {
            // Get the user ID and role from the authentication helper and session
            int? userId = _authHelper.GetUserId();
            string role = _authHelper.GetUserRole();

            // Create response with the current user's information
            CurrentUserResponse response = new CurrentUserResponse();
            response.UserId = userId;                                                   // Set the logged-in user's ID
            response.Role = role;                                                       // Set the logged-in user's role

            return response;
        }

        public class RegisterResponse
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class LoginResponse
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class CurrentUserResponse
        {
            public int? UserId { get; set; }
            public string Role { get; set; }
        }
    }
}