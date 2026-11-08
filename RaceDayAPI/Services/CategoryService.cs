using RaceDay.API.Data;
using RaceDay.API.DTO;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class CategoryService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;

        public CategoryService(RaceDayContext db, AuthenticationHelper authHelper, ValidationHelper validationHelper)
        {
            _db = db;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
        }

        public CategoryCreateResponse CreateCategory(int eventId, CategoryRequest request)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new CategoryCreateResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            RaceEvent parent = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (parent == null)
            {
                return new CategoryCreateResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return new CategoryCreateResponse { Error = "You can only add categories to your own events.", StatusCode = 403 };
            }

            string nameError = ValidationHelper.ValidateCategoryName(request.CategoryName);
            if (nameError != null)
            {
                return new CategoryCreateResponse { Error = nameError, StatusCode = 400 };
            }

            Category category = new Category();
            category.EventId = eventId;
            category.CategoryName = request.CategoryName;
            category.CreatedAt = DateTime.UtcNow;

            _db.Categories.Add(category);
            _db.SaveChanges();

            CategoryCreateResponse response = new CategoryCreateResponse();
            response.CategoryId = category.CategoryId;
            response.EventId = category.EventId;
            response.CategoryName = category.CategoryName;
            response.StatusCode = 201;

            return response;
        }

        public CategoryListResponse GetCategories(int eventId)
        {
            RaceEvent parent = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (parent == null)
            {
                return new CategoryListResponse { Error = "Event not found.", StatusCode = 404 };
            }

            List<Category> categories = _db.Categories.Where(c => c.EventId == eventId).ToList();

            CategoryListResponse response = new CategoryListResponse();
            response.Categories = new List<object>();

            foreach (Category c in categories)
            {
                object categoryObj = new
                {
                    category_id = c.CategoryId,
                    event_id = c.EventId,
                    category_name = c.CategoryName
                };
                response.Categories.Add(categoryObj);
            }

            response.StatusCode = 200;
            return response;
        }

        public CategoryDetailResponse GetCategory(int eventId, int categoryId)
        {
            Category found = _db.Categories.FirstOrDefault(c => c.CategoryId == categoryId && c.EventId == eventId);
            if (found == null)
            {
                return new CategoryDetailResponse { Error = "Category not found.", StatusCode = 404 };
            }

            CategoryDetailResponse response = new CategoryDetailResponse();
            response.CategoryId = found.CategoryId;
            response.EventId = found.EventId;
            response.CategoryName = found.CategoryName;
            response.StatusCode = 200;

            return response;
        }

        public CategoryUpdateResponse UpdateCategory(int eventId, int categoryId, CategoryRequest request)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new CategoryUpdateResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            RaceEvent parent = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (parent == null)
            {
                return new CategoryUpdateResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return new CategoryUpdateResponse { Error = "You can only change categories on your own events.", StatusCode = 403 };
            }

            Category found = _db.Categories.FirstOrDefault(c => c.CategoryId == categoryId && c.EventId == eventId);
            if (found == null)
            {
                return new CategoryUpdateResponse { Error = "Category not found.", StatusCode = 404 };
            }

            string nameError = ValidationHelper.ValidateCategoryName(request.CategoryName);
            if (nameError != null)
            {
                return new CategoryUpdateResponse { Error = nameError, StatusCode = 400 };
            }

            found.CategoryName = request.CategoryName;
            _db.SaveChanges();

            CategoryUpdateResponse response = new CategoryUpdateResponse();
            response.CategoryId = found.CategoryId;
            response.EventId = found.EventId;
            response.CategoryName = found.CategoryName;
            response.StatusCode = 200;

            return response;
        }

        public CategoryDeleteResponse DeleteCategory(int eventId, int categoryId)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new CategoryDeleteResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            RaceEvent parent = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (parent == null)
            {
                return new CategoryDeleteResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (parent.OrganiserId != organiser.OrganiserId)
            {
                return new CategoryDeleteResponse { Error = "You can only delete categories on your own events.", StatusCode = 403 };
            }

            Category found = _db.Categories.FirstOrDefault(c => c.CategoryId == categoryId && c.EventId == eventId);
            if (found == null)
            {
                return new CategoryDeleteResponse { Error = "Category not found.", StatusCode = 404 };
            }

            _db.Categories.Remove(found);
            _db.SaveChanges();

            CategoryDeleteResponse response = new CategoryDeleteResponse();
            response.StatusCode = 204;
            return response;
        }
    }

    public class CategoryCreateResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class CategoryListResponse
    {
        public List<object> Categories { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class CategoryDetailResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class CategoryUpdateResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }

    public class CategoryDeleteResponse
    {
        public string Error { get; set; }
        public int StatusCode { get; set; }
    }
}
