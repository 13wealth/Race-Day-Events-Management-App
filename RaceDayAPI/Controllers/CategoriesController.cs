using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manages race categories within events.
    /// Provides endpoints for creating, retrieving, updating, and deleting categories.
    /// </summary>
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesController"/> class.
        /// </summary>
        /// <param name="categoryService">The category service.</param>
        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Creates a new category for a specific event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="request">The category request containing category details.</param>
        /// <returns>
        /// A 201 Created response with the category details on success,
        /// a 400 Bad Request if validation fails,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Retrieves all categories for a specific event.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <returns>
        /// A 200 OK response with the list of categories,
        /// or a 404 Not Found if the event doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Retrieves a specific category by ID.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>
        /// A 200 OK response with the category details,
        /// or a 404 Not Found if the category doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Updates an existing category.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="request">The category request containing updated details.</param>
        /// <returns>
        /// A 200 OK response with the updated category details on success,
        /// a 400 Bad Request if validation fails,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the category doesn't exist.
        /// </returns>
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

        /// <summary>
        /// Deletes a category from an event.
        /// Only event organizers can perform this action.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>
        /// A 204 No Content response on successful deletion,
        /// a 403 Forbidden if user is not the event organizer,
        /// or a 404 Not Found if the category doesn't exist.
        /// </returns>
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
