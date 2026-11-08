using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Dtos;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

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
