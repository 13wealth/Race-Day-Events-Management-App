using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTO;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [RequireRole("organiser")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult CreateCategory(int eventId, [FromBody] CategoryRequest request)
        {
            CategoryService.CategoryCreateResponse response = _categoryService.CreateCategory(eventId, request);

            if (response.StatusCode == 400)
            {
                return BadRequest(new { error = response.Error });
            }

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return StatusCode(201, new
            {
                category_id = response.CategoryId,
                event_id = response.EventId,
                category_name = response.CategoryName
            });
        }

        [HttpGet]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetCategories(int eventId)
        {
            CategoryService.CategoryListResponse response = _categoryService.GetCategories(eventId);

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(response.Categories);
        }

        [HttpGet("{categoryId}")]
        [RequireRole]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetCategory(int eventId, int categoryId)
        {
            CategoryService.CategoryDetailResponse response = _categoryService.GetCategory(eventId, categoryId);

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(new
            {
                category_id = response.CategoryId,
                event_id = response.EventId,
                category_name = response.CategoryName
            });
        }

        [HttpPut("{categoryId}")]
        [RequireRole("organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult UpdateCategory(int eventId, int categoryId, [FromBody] CategoryRequest request)
        {
            CategoryService.CategoryUpdateResponse response = _categoryService.UpdateCategory(eventId, categoryId, request);

            if (response.StatusCode == 400)
            {
                return BadRequest(new { error = response.Error });
            }

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return Ok(new
            {
                category_id = response.CategoryId,
                event_id = response.EventId,
                category_name = response.CategoryName
            });
        }

        [HttpDelete("{categoryId}")]
        [RequireRole("organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteCategory(int eventId, int categoryId)
        {
            CategoryService.CategoryDeleteResponse response = _categoryService.DeleteCategory(eventId, categoryId);

            if (response.StatusCode == 403)
            {
                return StatusCode(403, new { error = response.Error });
            }

            if (response.StatusCode == 404)
            {
                return NotFound(new { error = response.Error });
            }

            return NoContent();
        }
    }
}
