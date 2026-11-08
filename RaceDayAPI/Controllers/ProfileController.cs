using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages user profile operations.
    /// Provides endpoints for retrieving and updating user profile information.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class.
        /// </summary>
        /// <param name="profileService">The profile service.</param>
        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A 200 OK response with the user's profile information,
        /// a 401 Unauthorized if user is not authenticated,
        /// or a 404 Not Found if the profile doesn't exist.
        /// </returns>
        [HttpGet]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetMyProfile()
        {
            ProfileService.ProfileGetResponse response = _profileService.GetMyProfile();

            if (response.StatusCode == 401)
            {
                return Unauthorized(new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(response.Profile);
        }

        /// <summary>
        /// Updates the profile information of the currently authenticated user.
        /// </summary>
        /// <param name="request">The profile request containing updated profile details.</param>
        /// <returns>
        /// A 200 OK response with the updated profile information,
        /// a 401 Unauthorized if user is not authenticated,
        /// or a 404 Not Found if the profile doesn't exist.
        /// </returns>
        [HttpPut]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult UpdateMyProfile([FromBody] ProfileRequest request)
        {
            ProfileService.ProfileUpdateResponse response = _profileService.UpdateMyProfile(request);

            if (response.StatusCode == 401)
            {
                return Unauthorized(new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(response.Profile);
        }
    }
}
