using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.Dtos;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayContext _db;

        public ProfileController(RaceDayContext db)
        {
            _db = db;
        }

        /*
         * Finds the user's profile and their role record (organiser or participant)
         * and returns the name and phone fields as a list of values.
         * The method below uses the session to find the user.
         */
        private object GetProfileResponse(int userId, string role)
        {
            if (role == "organiser")
            {
                Organiser organiser = (from o in _db.Organisers
                                       join p in _db.Profiles on o.ProfileId equals p.ProfileId
                                       where p.UserId == userId
                                       select o).FirstOrDefault();

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
                Participant participant = (from p in _db.Participants
                                           join pr in _db.Profiles on p.ProfileId equals pr.ProfileId
                                           where pr.UserId == userId
                                           select p).FirstOrDefault();

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

        /// <summary>
        /// Gets the logged-in user's profile. Only the logged-in user can view their own profile.
        /// </summary>
        [HttpGet]
        [RequireRole]
        public IActionResult GetMyProfile()
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string role = HttpContext.Session.GetString("Role");

            object profile = GetProfileResponse(userId, role);
            if (profile == null)
            {
                return NotFound(new { error = "Profile not found." });
            }

            return Ok(profile);
        }

        /// <summary>
        /// Updates the logged-in user’s first name, last name and phone. Any logged-in user.
        /// </summary>
        [HttpPut]
        [RequireRole]
        public IActionResult UpdateMyProfile([FromBody] ProfileRequest request)
        {
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string role = HttpContext.Session.GetString("Role");

            if (role == "organiser")
            {
                Organiser organiser = (from o in _db.Organisers
                                       join p in _db.Profiles on o.ProfileId equals p.ProfileId
                                       where p.UserId == userId
                                       select o).FirstOrDefault();

                if (organiser == null)
                {
                    return NotFound(new { error = "Profile not found." });
                }

                organiser.FirstName = request.FirstName;
                organiser.LastName = request.LastName;
                organiser.Phone = request.Phone;
                organiser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                Participant participant = (from p in _db.Participants
                                           join pr in _db.Profiles on p.ProfileId equals pr.ProfileId
                                           where pr.UserId == userId
                                           select p).FirstOrDefault();

                if (participant == null)
                {
                    return NotFound(new { error = "Profile not found." });
                }

                participant.FirstName = request.FirstName;
                participant.LastName = request.LastName;
                participant.Phone = request.Phone;
                participant.UpdatedAt = DateTime.UtcNow;
            }

            _db.SaveChanges();

            object updated = GetProfileResponse(userId, role);
            return Ok(updated);
        }
    }
}