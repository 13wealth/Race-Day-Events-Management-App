using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.DTO;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using static Azure.Core.HttpHeader;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _db;

        public CategoriesController(RaceDayContext db)
        {
            _db = db;
        }

        /*
         * Helper methods to get the logged-in organiser and find an event by id
         */
        private Organiser GetLoggedInOrganiser()
        {
            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (userIdNullable == null)
            {
                return null;
            }
            int userId = userIdNullable.Value;

            Organiser organiser = (from o in _db.Organisers
                                   join p in _db.Profiles on o.ProfileId equals p.ProfileId
                                   where p.UserId == userId
                                   select o).FirstOrDefault();

            return organiser;
        }

        /*
         * Helper method to find an event by id
         */
        private RaceEvent FindEvent(int eventId)
        {
            RaceEvent found = (from e in _db.Events
                               where e.EventId == eventId
                               select e).FirstOrDefault();

            if (found == null)
            {
                return null;
            }

            return found;
        }

        /*
         * Builds the JSON shape returned for one category
         */
        private object CategoryToResponse(Category c)
        {
            return new
            {
                category_id = c.CategoryId,
                event_id = c.EventId,
                category_name = c.CategoryName
            };
        }

        /// <summary>
        /// Adds a category to an event. Only the organiser who owns the event.
        /// </summary>
        [HttpPost]
        [RequireRole("organiser")]
        public IActionResult CreateCategory(int eventId, [FromBody] CategoryRequest request)
        {
            Organiser organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            RaceEvent parent = FindEvent(eventId);
            if (parent == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only add categories to your own events." });
            }

            Category category = new Category();
            category.EventId = eventId;
            category.CategoryName = request.CategoryName;
            category.CreatedAt = DateTime.UtcNow;

            _db.Categories.Add(category);
            _db.SaveChanges();

            return StatusCode(201, CategoryToResponse(category));
        }

        /// <summary>
        /// Lists the categories for an event. Any logged-in user.
        /// </summary>
        [HttpGet]
        [RequireRole]
        public IActionResult GetCategories(int eventId)
        {
            RaceEvent parent = FindEvent(eventId);
            if (parent == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            List<Category> categories = (from c in _db.Categories
                                         where c.EventId == eventId
                                         select c).ToList();

            List<object> result = new List<object>();
            foreach (Category c in categories)
            {
                result.Add(CategoryToResponse(c));
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns one category by id. Any logged-in user.
        /// </summary>
        [HttpGet("{categoryId}")]
        [RequireRole]
        public IActionResult GetCategory(int eventId, int categoryId)
        {
            Category found = (from c in _db.Categories
                              where c.CategoryId == categoryId && c.EventId == eventId
                              select c).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            return Ok(CategoryToResponse(found));
        }

        /// <summary>
        /// Renames a category. Only the organiser who owns the event.
        /// </summary>
        [HttpPut("{categoryId}")]
        [RequireRole("organiser")]
        public IActionResult UpdateCategory(int eventId, int categoryId, [FromBody] CategoryRequest request)
        {
            Organiser organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            RaceEvent parent = FindEvent(eventId);
            if (parent == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only change categories on your own events." });
            }

            Category found = (from c in _db.Categories
                              where c.CategoryId == categoryId && c.EventId == eventId
                              select c).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            found.CategoryName = request.CategoryName;
            _db.SaveChanges();

            return Ok(CategoryToResponse(found));
        }

        /// <summary>
        /// Deletes a category. Only the organiser who owns the event.
        /// </summary>
        [HttpDelete("{categoryId}")]
        [RequireRole("organiser")]
        public IActionResult DeleteCategory(int eventId, int categoryId)
        {
            Organiser organiser = GetLoggedInOrganiser();
            if (organiser == null)
            {
                return NotFound(new { error = "Organiser profile not found." });
            }

            RaceEvent parent = FindEvent(eventId);
            if (parent == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return StatusCode(403, new { error = "You can only delete categories on your own events." });
            }

            Category found = (from c in _db.Categories
                              where c.CategoryId == categoryId && c.EventId == eventId
                              select c).FirstOrDefault();

            if (found == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            _db.Categories.Remove(found);
            _db.SaveChanges();

            return NoContent();
        }
    }
}