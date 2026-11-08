using RaceDay.API.Data;
using RaceDay.API.Dtos;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class ProfileService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;

        public ProfileService(RaceDayContext db, AuthenticationHelper authHelper)
        {
            _db = db;
            _authHelper = authHelper;
        }

        private object GetProfileResponse(int userId, string role)
        {
            if (role == "organiser")
            {
                Organiser organiser = _db.Organisers.FirstOrDefault(o => 
                    _db.Profiles.Any(p => p.ProfileId == o.ProfileId && p.UserId == userId)
                );

                if (organiser == null)
                {
                    return null;
                }

                return new
                {
                    user_id = userId,
                    role = role,
                    first_name = organiser.FirstName,
                    last_name = organiser.LastName,
                    phone = organiser.Phone
                };
            }
            else
            {
                Participant participant = _db.Participants.FirstOrDefault(p => 
                    _db.Profiles.Any(pr => pr.ProfileId == p.ProfileId && pr.UserId == userId)
                );

                if (participant == null)
                {
                    return null;
                }

                return new
                {
                    user_id = userId,
                    role = role,
                    first_name = participant.FirstName,
                    last_name = participant.LastName,
                    phone = participant.Phone
                };
            }
        }

        public ProfileGetResponse GetMyProfile()
        {
            int? userIdNullable = _authHelper.GetUserId();
            if (userIdNullable == null)
            {
                return new ProfileGetResponse { Error = "User not logged in.", StatusCode = 401 };
            }

            int userId = userIdNullable.Value;
            string role = _authHelper.GetUserRole();

            object profile = GetProfileResponse(userId, role);
            if (profile == null)
            {
                return new ProfileGetResponse { Error = "Profile not found.", StatusCode = 404 };
            }

            ProfileGetResponse response = new ProfileGetResponse();
            response.Profile = profile;
            response.StatusCode = 200;

            return response;
        }

        public ProfileUpdateResponse UpdateMyProfile(ProfileRequest request)
        {
            int? userIdNullable = _authHelper.GetUserId();
            if (userIdNullable == null)
            {
                return new ProfileUpdateResponse { Error = "User not logged in.", StatusCode = 401 };
            }

            int userId = userIdNullable.Value;
            string role = _authHelper.GetUserRole();

            if (role == "organiser")
            {
                Organiser organiser = _db.Organisers.FirstOrDefault(o => 
                    _db.Profiles.Any(p => p.ProfileId == o.ProfileId && p.UserId == userId)
                );

                if (organiser == null)
                {
                    return new ProfileUpdateResponse { Error = "Profile not found.", StatusCode = 404 };
                }

                organiser.FirstName = request.FirstName;
                organiser.LastName = request.LastName;
                organiser.Phone = request.Phone;
                organiser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                Participant participant = _db.Participants.FirstOrDefault(p => 
                    _db.Profiles.Any(pr => pr.ProfileId == p.ProfileId && pr.UserId == userId)
                );

                if (participant == null)
                {
                    return new ProfileUpdateResponse { Error = "Profile not found.", StatusCode = 404 };
                }

                participant.FirstName = request.FirstName;
                participant.LastName = request.LastName;
                participant.Phone = request.Phone;
                participant.UpdatedAt = DateTime.UtcNow;
            }

            _db.SaveChanges();

            object updated = GetProfileResponse(userId, role);

            ProfileUpdateResponse response = new ProfileUpdateResponse();
            response.Profile = updated;
            response.StatusCode = 200;

            return response;
        }
    }

    public class ProfileGetResponse
    {
        public object Profile { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class ProfileUpdateResponse
    {
        public object Profile { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }
}
