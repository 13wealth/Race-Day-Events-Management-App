using RaceDay.API.Data;
using RaceDay.Contracts;
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

        /*
         * Retrieves the profile of a user based on their ID and role.
         * Searches the database for the organiser or participant associated with the user ID.
         * Converts the user profile to an anonymous object format for API response.
         * Returns an anonymous object with user details if found, otherwise returns null.
         */
        private object GetProfileResponse(int userId, string role)
        {
            if (role == "organiser")                                                    // Check if user role is organiser
            {
                // Search for organiser with matching user ID
                Organiser organiser = null;                                             // Initialize organiser to null
                foreach (Organiser o in _db.Organisers)
                {
                    // Check if this organiser's profile matches the user ID
                    bool found = false;                                                 // Initialize found flag to false
                    foreach (Profile p in _db.Profiles)
                    {
                        if (p.ProfileId == o.ProfileId && p.UserId == userId)           // Check if profile ID and user ID both match
                        {
                            found = true;                                               // Mark as found
                            break;                                                      // Exit the inner loop immediately
                        }
                    }
                    if (found)                                                          // If profile was found for this organiser
                    {
                        organiser = o;                                                  // Store the organiser
                        break;                                                          // Exit the outer loop immediately
                    }
                }

                if (organiser == null)                                                  // If no organiser was found with matching user ID
                {
                    return null;
                }

                // Create anonymous object with organiser profile details
                object result = new
                {
                    user_id = userId,
                    role = role,
                    first_name = organiser.FirstName,
                    last_name = organiser.LastName,
                    phone = organiser.Phone
                };
                return result;
            }
            else                                                                        // User role is participant
            {
                // Search for participant with matching user ID
                Participant participant = null;                                         // Initialize participant to null
                foreach (Participant p in _db.Participants)
                {
                    // Check if this participant's profile matches the user ID
                    bool found = false;                                                 // Initialize found flag to false
                    foreach (Profile pr in _db.Profiles)
                    {
                        if (pr.ProfileId == p.ProfileId && pr.UserId == userId)         // Check if profile ID and user ID both match
                        {
                            found = true;                                               // Mark as found
                            break;                                                      // Exit the inner loop immediately
                        }
                    }
                    if (found)                                                          // If profile was found for this participant
                    {
                        participant = p;                                                // Store the participant
                        break;                                                          // Exit the outer loop immediately
                    }
                }

                if (participant == null)                                                // If no participant was found with matching user ID
                {
                    return null;
                }

                object result = new
                {
                    user_id = userId,
                    role = role,
                    first_name = participant.FirstName,
                    last_name = participant.LastName,
                    phone = participant.Phone
                };
                return result;
            }
        }

        /*
         * Retrieves the profile of the currently logged-in user.
         * Verifies that a user is logged in by checking session data.
         * Calls GetProfileResponse to retrieve the profile based on user ID and role.
         * Returns a ProfileGetResponse with user details and 200 status code on success.
         * Returns error responses with appropriate status codes (401, 404) for authentication or profile lookup failures.
         */
        public ProfileGetResponse GetMyProfile()
        {
            // Retrieve the logged-in user ID from the authentication helper and session
            int? userIdNullable = _authHelper.GetUserId();
            if (userIdNullable == null)                                                 // Check if user is not logged in
            {
                ProfileGetResponse notLoggedInResponse = new ProfileGetResponse();
                notLoggedInResponse.Error = "User not logged in.";
                notLoggedInResponse.StatusCode = 401;
                return notLoggedInResponse;
            }

            // Extract user ID value and retrieve user role
            int userId = userIdNullable.Value;
            string role = _authHelper.GetUserRole();

            // Retrieve the user's profile using the helper method
            object profile = GetProfileResponse(userId, role);
            if (profile == null)                                                        // If profile was not found
            {
                ProfileGetResponse notFoundResponse = new ProfileGetResponse();
                notFoundResponse.Error = "Profile not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            // Create success response with the profile details
            ProfileGetResponse response = new ProfileGetResponse();
            response.Profile = profile;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Updates the profile of the currently logged-in user.
         * Retrieves the user ID and role from the authentication helper and session.
         * Searches for the organiser or participant record and updates their profile fields.
         * Returns a ProfileUpdateResponse with updated profile details and 200 status code on success.
         * Returns error responses with appropriate status codes (401, 404) for authentication or profile lookup failures.
         */
        public ProfileUpdateResponse UpdateMyProfile(ProfileRequest request)
        {
            // Retrieve the logged-in user ID from the authentication helper and session
            int? userIdNullable = _authHelper.GetUserId();
            if (userIdNullable == null)                                                 // Check if user is not logged in
            {
                ProfileUpdateResponse notLoggedInResponse = new ProfileUpdateResponse();
                notLoggedInResponse.Error = "User not logged in.";
                notLoggedInResponse.StatusCode = 401;
                return notLoggedInResponse;
            }

            int userId = userIdNullable.Value;
            string role = _authHelper.GetUserRole();

            if (role == "organiser")
            {
                Organiser organiser = null;
                foreach (Organiser o in _db.Organisers)
                {
                    bool found = false;
                    foreach (Profile p in _db.Profiles)
                    {
                        if (p.ProfileId == o.ProfileId && p.UserId == userId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        organiser = o;
                        break;
                    }
                }

                if (organiser == null)
                {
                    ProfileUpdateResponse notFoundResponse = new ProfileUpdateResponse();
                    notFoundResponse.Error = "Profile not found.";
                    notFoundResponse.StatusCode = 404;
                    return notFoundResponse;
                }

                organiser.FirstName = request.FirstName;
                organiser.LastName = request.LastName;
                organiser.Phone = request.Phone;
                organiser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                Participant participant = null;
                foreach (Participant p in _db.Participants)
                {
                    bool found = false;
                    foreach (Profile pr in _db.Profiles)
                    {
                        if (pr.ProfileId == p.ProfileId && pr.UserId == userId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        participant = p;
                        break;
                    }
                }

                if (participant == null)
                {
                    ProfileUpdateResponse notFoundResponse = new ProfileUpdateResponse();
                    notFoundResponse.Error = "Profile not found.";
                    notFoundResponse.StatusCode = 404;
                    return notFoundResponse;
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
}
