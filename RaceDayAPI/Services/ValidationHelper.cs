using RaceDay.Contracts;

namespace RaceDay.API.Services
{
    public class ValidationHelper
    {
        /*
         * Validates the user's role by checking whether it is either "organiser" or "participant"
         * Checks if role exsist, if not returns an error
         */
        public static string ValidateRole(string role)
        {
            string roleLower = role.ToLower();                                          // Converts the role to lowercase
            if (roleLower != "organiser" && roleLower != "participant") 
            {
                return "Role must be 'organiser' or 'participant'.";
            }
            return null;
        }


        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "Password cannot be empty.";
            }
            if (password.Length < 8)
            {
                return "Password must be at least 8 characters.";
            }
            return null;
        }

        public static string ValidateEventType(string eventType)
        {
            if (eventType != "run" && eventType != "walk" && eventType != "cycle")
            {
                return "Event type must be 'run', 'walk', or 'cycle'.";
            }
            return null;
        }

        public static string ValidateEventStatus(string status)
        {
            if (status != "scheduled" && status != "ongoing" && status != "completed" && status != "cancelled")
            {
                return "Status must be 'scheduled', 'ongoing', 'completed', or 'cancelled'.";
            }
            return null;
        }

        public static string ValidateCategoryName(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return "Category name cannot be empty.";
            }
            return null;
        }

        public static string ValidateEventRequest(EventRequest request)
        {
            if (string.IsNullOrEmpty(request.EventName))
            {
                return "Event name cannot be empty.";
            }
            if (string.IsNullOrEmpty(request.Location))
            {
                return "Location cannot be empty.";
            }
            string typeError = ValidateEventType(request.EventType);
            if (typeError != null)
            {
                return typeError;
            }
            return null;
        }

        public static string ValidateCategoryRequest(CategoryRequest request)
        {
            if (string.IsNullOrEmpty(request.CategoryName))
            {
                return "Category name cannot be empty.";
            }
            return null;
        }

        public static string ValidateEnrollmentRequest(EnrollmentRequest request)
        {
            if (request.CategoryId <= 0)
            {
                return "Invalid category ID.";
            }
            return null;
        }
    }
}