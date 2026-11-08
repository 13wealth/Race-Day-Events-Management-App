using RaceDay.API.Data;
using RaceDay.Contracts;
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

        /*
         * Creates a new category for a specific event.
         * Verifies that the logged-in user is an organiser and owns the event before allowing category creation.
         * Validates the category name using ValidationHelper methods.
         * Returns a CategoryCreateResponse with category details and 201 status code on success.
         * Returns error responses with appropriate status codes (404, 403, 400) for validation or authorization failures.
         */
        public CategoryCreateResponse CreateCategory(int eventId, CategoryRequest request)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                CategoryCreateResponse organiserErrorResponse = new CategoryCreateResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Search for the event that will contain this category
            RaceEvent parent = null;                                                    // Initialize parent event to null
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    parent = e;                                                         // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (parent == null)                                                         // If no event was found with that ID
            {
                CategoryCreateResponse eventNotFoundResponse = new CategoryCreateResponse();
                eventNotFoundResponse.Error = "Event not found.";
                eventNotFoundResponse.StatusCode = 404;
                return eventNotFoundResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from adding categories to other organisers' events.
             */
            if (parent.OrganiserId != organiser.OrganiserId)                           // Check if organiser does not own this event
            {
                CategoryCreateResponse forbiddenResponse = new CategoryCreateResponse();
                forbiddenResponse.Error = "You can only add categories to your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Validate the category name provided in the request
            string nameError = ValidationHelper.ValidateCategoryName(request.CategoryName);
            if (nameError != null)                                                      // Check if category name validation failed
            {
                CategoryCreateResponse nameErrorResponse = new CategoryCreateResponse();
                nameErrorResponse.Error = nameError;
                nameErrorResponse.StatusCode = 400;
                return nameErrorResponse;
            }

            // Create a new category object for the event
            Category category = new Category();
            category.EventId = eventId;
            category.CategoryName = request.CategoryName;
            category.CreatedAt = DateTime.UtcNow;

            // Add the new category to the database and save changes
            _db.Categories.Add(category);
            _db.SaveChanges();

            // Create success response with the created category details
            CategoryCreateResponse response = new CategoryCreateResponse();
            response.CategoryId = category.CategoryId;
            response.EventId = category.EventId;
            response.CategoryName = category.CategoryName;
            response.StatusCode = 201;

            return response;
        }

        /*
         * Retrieves all categories for a specific event.
         * Returns a list of category objects with category details.
         * Converts each category to an anonymous object format for API response.
         * Returns a CategoryListResponse with 200 status code on success.
         * Returns error response with 404 status code if event is not found.
         */
        public CategoryListResponse GetCategories(int eventId)
        {
            // Search for the event to retrieve its categories
            RaceEvent parent = null;                                                    // Initialize event to null
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    parent = e;                                                         // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (parent == null)                                                          // If no event was found with that ID
            {
                CategoryListResponse notFoundResponse = new CategoryListResponse();
                notFoundResponse.Error = "Event not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            // Retrieve all categories that belong to the event and add them to a list
            List<Category> categories = new List<Category>();
            foreach (Category c in _db.Categories)
            {
                if (c.EventId == eventId)                                               // Check if category belongs to this event
                {
                    categories.Add(c);
                }
            }

            // Create response object and initialize the categories list
            CategoryListResponse response = new CategoryListResponse();
            response.Categories = new List<object>();

            /*
             * Iterate through all categories and convert each to an anonymous object format.
             * This format is used for the API response to format field names appropriately.
             */
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

        /*
         * Retrieves a specific category by its category ID and event ID.
         * Verifies that the category belongs to the specified event.
         * Searches the database for the category matching both provided IDs.
         * Returns a CategoryDetailResponse with category details and 200 status code on success.
         * Returns error response with 404 status code if category or event is not found.
         */
        public CategoryDetailResponse GetCategory(int eventId, int categoryId)
        {
            // Search for a category with the matching category ID and event ID in the database
            Category found = null;                                                      // Initialize found category to null
            foreach (Category c in _db.Categories)
            {
                if (c.CategoryId == categoryId && c.EventId == eventId)                 // Check if both category ID and event ID match
                {
                    found = c;                                                          // Found the category
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no category was found matching both IDs
            {
                CategoryDetailResponse notFoundResponse = new CategoryDetailResponse();
                notFoundResponse.Error = "Category not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            // Create success response with the category details
            CategoryDetailResponse response = new CategoryDetailResponse();
            response.CategoryId = found.CategoryId;
            response.EventId = found.EventId;
            response.CategoryName = found.CategoryName;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Updates an existing category with new information.
         * Verifies that the logged-in user is an organiser and owns the event before allowing category updates.
         * Validates the new category name using ValidationHelper methods.
         * Returns a CategoryUpdateResponse with updated category details and 200 status code on success.
         * Returns error responses with appropriate status codes (404, 403, 400) for authorization or validation failures.
         */
        public CategoryUpdateResponse UpdateCategory(int eventId, int categoryId, CategoryRequest request)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                CategoryUpdateResponse organiserErrorResponse = new CategoryUpdateResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Search for the event that contains this category
            RaceEvent parent = null;                                                    // Initialize event to null
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    parent = e;                                                         // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (parent == null)                                                         // If no event was found with that ID
            {
                CategoryUpdateResponse eventNotFoundResponse = new CategoryUpdateResponse();
                eventNotFoundResponse.Error = "Event not found.";
                eventNotFoundResponse.StatusCode = 404;
                return eventNotFoundResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from updating categories within other organisers' events.
             */
            if (parent.OrganiserId != organiser.OrganiserId)                           // Check if organiser does not own this event
            {
                CategoryUpdateResponse forbiddenResponse = new CategoryUpdateResponse();
                forbiddenResponse.Error = "You can only change categories on your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Search for the category to be updated by its category ID and event ID
            Category found = null;                                                      // Initialize found category to null
            foreach (Category c in _db.Categories)
            {
                if (c.CategoryId == categoryId && c.EventId == eventId)                 // Check if both category ID and event ID match
                {
                    found = c;                                                          // Found the category
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no category was found matching both IDs
            {
                CategoryUpdateResponse categoryNotFoundResponse = new CategoryUpdateResponse();
                categoryNotFoundResponse.Error = "Category not found.";
                categoryNotFoundResponse.StatusCode = 404;
                return categoryNotFoundResponse;
            }

            // Validate the new category name provided in the request
            string nameError = ValidationHelper.ValidateCategoryName(request.CategoryName);
            if (nameError != null)                                                      // Check if category name validation failed
            {
                CategoryUpdateResponse nameErrorResponse = new CategoryUpdateResponse();
                nameErrorResponse.Error = nameError;
                nameErrorResponse.StatusCode = 400;
                return nameErrorResponse;
            }

            found.CategoryName = request.CategoryName;
            _db.SaveChanges();


            // Update the category name
            found.CategoryName = request.CategoryName;

            // Create success response with the updated category details
            CategoryUpdateResponse response = new CategoryUpdateResponse();
            response.CategoryId = found.CategoryId;
            response.EventId = found.EventId;
            response.CategoryName = found.CategoryName;
            response.StatusCode = 200;

            return response;
        }

        /*
         * Deletes a category from the database.
         * Verifies that the logged-in user is an organiser and owns the event before allowing category deletion.
         * Removes the category record from the database permanently.
         * Returns a CategoryDeleteResponse with 204 status code (No Content) on success.
         * Returns error responses with appropriate status codes (404, 403) for authorization or validation failures.
         */
        public CategoryDeleteResponse DeleteCategory(int eventId, int categoryId)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                CategoryDeleteResponse organiserErrorResponse = new CategoryDeleteResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            RaceEvent parent = null;
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == eventId)                                               // Check if event ID matches
                {
                    parent = e;                                                         // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (parent == null)                                                         // If no event was found with that ID
            {
                CategoryDeleteResponse eventNotFoundResponse = new CategoryDeleteResponse();
                eventNotFoundResponse.Error = "Event not found.";
                eventNotFoundResponse.StatusCode = 404;
                return eventNotFoundResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from deleting categories within other organisers' events.
             */
            if (parent.OrganiserId != organiser.OrganiserId)                           // Check if organiser does not own this event
            {
                CategoryDeleteResponse forbiddenResponse = new CategoryDeleteResponse();
                forbiddenResponse.Error = "You can only delete categories on your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Search for the category to be deleted by its category ID and event ID
            Category found = null;                                                      // Initialize found category to null
            foreach (Category c in _db.Categories)
            {
                if (c.CategoryId == categoryId && c.EventId == eventId)                 // Check if both category ID and event ID match
                {
                    found = c;                                                          // Found the category
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no category was found matching both IDs
            {
                CategoryDeleteResponse categoryNotFoundResponse = new CategoryDeleteResponse();
                categoryNotFoundResponse.Error = "Category not found.";
                categoryNotFoundResponse.StatusCode = 404;
                return categoryNotFoundResponse;
            }

            // Remove the category from the database and save changes
            _db.Categories.Remove(found);
            _db.SaveChanges();

            // Create response with 204 No Content status code
            CategoryDeleteResponse response = new CategoryDeleteResponse();
            response.StatusCode = 204;
            return response;
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
}
