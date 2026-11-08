using Microsoft.AspNetCore.Http;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class AuthenticationHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RaceDayContext _db;

        public AuthenticationHelper(IHttpContextAccessor httpContextAccessor, RaceDayContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public int? GetUserId()
        {
            int? userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            return userId;
        }

        public string GetUserRole()
        {
            string role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            return role;
        }

        public Organiser GetLoggedInOrganiser()
        {
            int? userId = GetUserId();
            if (userId == null)
            {
                return null;
            }

            foreach (var organiser in _db.Organisers)
            {
                if (organiser.ProfileId > 0)
                {
                    Profile profile = _db.Profiles.FirstOrDefault(p => p.ProfileId == organiser.ProfileId);
                    if (profile != null && profile.UserId == userId.Value)
                    {
                        return organiser;
                    }
                }
            }

            return null;
        }

        public Participant GetLoggedInParticipant()
        {
            int? userId = GetUserId();
            if (userId == null)
            {
                return null;
            }

            foreach (var participant in _db.Participants)
            {
                if (participant.ProfileId > 0)
                {
                    Profile profile = _db.Profiles.FirstOrDefault(p => p.ProfileId == participant.ProfileId);
                    if (profile != null && profile.UserId == userId.Value)
                    {
                        return participant;
                    }
                }
            }

            return null;
        }

        public void SetSession(int userId, string role)
        {
            _httpContextAccessor.HttpContext.Session.SetInt32("UserId", userId);
            _httpContextAccessor.HttpContext.Session.SetString("Role", role);
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }
    }
}