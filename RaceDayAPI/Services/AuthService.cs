using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTO;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class AuthService
    {
        private readonly RaceDayContext _db;
        private readonly IPasswordHasher<RaceDayUser> _passwordHasher;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;

        public AuthService(RaceDayContext db, IPasswordHasher<RaceDayUser> passwordHasher, AuthenticationHelper authHelper, ValidationHelper validationHelper)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate role
            string roleError = ValidationHelper.ValidateRole(request.Role);
            if (roleError != null)
            {
                return new RegisterResponse { Error = roleError, StatusCode = 400 };
            }

            // Validate password
            string passwordError = ValidationHelper.ValidatePassword(request.Password);
            if (passwordError != null)
            {
                return new RegisterResponse { Error = passwordError, StatusCode = 400 };
            }

            string role = request.Role.ToLower();

            // Check if username or email already exists
            RaceDayUser existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (existing != null)
            {
                return new RegisterResponse { Error = "Username or email already exists.", StatusCode = 409 };
            }

            DateTime now = DateTime.UtcNow;

            // Create user
            RaceDayUser user = new RaceDayUser();
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = role;
            user.CreatedAt = now;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Create profile
            Profile profile = new Profile();
            profile.User = user;
            profile.CreatedAt = now;
            user.Profile = profile;

            // Create role-specific entity
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

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            RaceDayUser user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return new LoginResponse { Error = "Invalid username or password.", StatusCode = 401 };
            }

            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return new LoginResponse { Error = "Invalid username or password.", StatusCode = 401 };
            }

            // Set session
            _authHelper.SetSession(user.UserId, user.Role);

            LoginResponse response = new LoginResponse();
            response.UserId = user.UserId;
            response.Username = user.Username;
            response.Email = user.Email;
            response.Role = user.Role;
            response.StatusCode = 200;

            return response;
        }

        public void Logout()
        {
            _authHelper.ClearSession();
        }

        public CurrentUserResponse GetCurrentUser()
        {
            int? userId = _authHelper.GetUserId();
            string role = _authHelper.GetUserRole();

            CurrentUserResponse response = new CurrentUserResponse();
            response.UserId = userId;
            response.Role = role;

            return response;
        }
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