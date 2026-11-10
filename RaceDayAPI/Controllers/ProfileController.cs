using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;
using RaceDay.API.Storage;

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
        private readonly IBlobStorageService _blobStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class.
        /// </summary>
        /// <param name="profileService">The profile service.</param>
        /// <param name="blobStorageService">The blob storage service.</param>
        public ProfileController(ProfileService profileService, 
                                 IBlobStorageService blobStorageService
            )
        {
            _profileService = profileService;
            _blobStorageService = blobStorageService;
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

        /// <summary>
        /// Uploads a profile image for the currently authenticated user.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("image")]
        [RequireRole]
        public async Task<IActionResult> UploadProfileImage(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Please select an image." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            try
            {
                string imageUrl =
                    await _blobStorageService.UploadProfileImageAsync(
                        file, userId.Value);

                var result = _profileService.UpdateProfileImage(imageUrl);

                return StatusCode(result.StatusCode, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
