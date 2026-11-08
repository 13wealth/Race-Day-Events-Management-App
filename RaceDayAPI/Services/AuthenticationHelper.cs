using Microsoft.AspNetCore.Http;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class AuthenticationHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;                            // This is used to access the current HTTP context, which includes session data.
        private readonly RaceDayContext _db;                                                   // This is the database context used to interact with the database.

        public AuthenticationHelper(IHttpContextAccessor httpContextAccessor, 
                                    RaceDayContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        /*
         * Retrieves the user ID from the session.
         * Returns a nullable integer representing the user ID.
         */
        public Nullable<int> GetUserId()
        {
            Nullable<int> userId = _httpContextAccessor
                                   .HttpContext
                                   .Session
                                   .GetInt32("UserId");
            return userId;
        }

        /*
         * Retrieves the role of the logged-in user from the session.
         * Returns a string representing the user's role.
         */
        public string GetUserRole()
        {
            string role = _httpContextAccessor
                          .HttpContext.Session
                          .GetString("Role");
            return role;
        }

        /*
         * Retrieves the logged-in organiser based on the user ID stored in the session.
         * Returns an Organiser object if found, otherwise returns null.
         */
        public Organiser GetLoggedInOrganiser()
        {
            Nullable<int> userId = GetUserId();                                         // Get the user ID from the session
            
            if (userId == null)
            {
                return null;
            }

            List<Organiser> organisers = _db.Organisers.ToList();                   // Retrieve all organisers from the database
            List<Profile> profiles = _db.Profiles.ToList();                         // Retrieve all profiles from the database

            /*
             * Iterate through all organisers in memory instead of the database.
             * For each organiser, check if they have a valid profile ID.
             * If they do, retrieve the associated profile and check if the user ID matches the logged-in user ID.
             * If a match is found, return the organiser.
             */
            foreach (Organiser organiser in organisers)
            {
                if (organiser.ProfileId > 0)
                {
                    Profile profile = null;                                          // Initialize profile to null

                    /*
                     * Iterate through all profiles in the database to find the profile associated with the organiser.
                     * If a matching profile is found, break the loop and assign it to the profile variable.
                     */
                    foreach (Profile item in profiles)
                    {
                        if (item.ProfileId == organiser.ProfileId)
                        {
                            profile = item;
                            break;
                        }
                    }

                    if (profile != null && profile.UserId == userId.Value)          // Check if the profile is not null and if the user ID matches the logged-in user ID
                    {
                        return organiser;
                    }
                }
            }

            return null;
        }

        /*
         * Retrieves the logged-in participant based on the UserId in session
         * Returns an Participant object if found, otherwise returns null.
         */
        public Participant GetLoggedInParticipant()
        {
            Nullable<int> userId = GetUserId();
            if (userId == null)
            {
                return null;
            }

            List<Participant> participants = _db.Participants.ToList();                     // Retrieve all participants from the database
            List<Profile> profiles = _db.Profiles.ToList();                                 // Retrieve all profiles from the database

            foreach (var participant in participants)
            {
                if (participant.ProfileId > 0)
                {
                    Profile profile = null;
                    
                    foreach (Profile item in profiles)
                    {
                        if (item.ProfileId == participant.ProfileId)
                        {
                            profile = item;
                            break;
                        }
                    }

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