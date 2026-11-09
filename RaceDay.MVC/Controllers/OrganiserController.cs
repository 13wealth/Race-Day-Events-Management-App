
/***************************************************************************************
 *                                ORGANISER CONTROLLER                                 *
 * This controller manages the organiser section of the RaceDay MVC application.       *
 * It handles operations such as:                                                      *
 *  1. Dashboard – displays the organiser dashboard.                                   *
 *  2. Access Control – ensures only authenticated organisers access these pages.      *
 *  3. Event Management – manages race events.                                         *
 *  4. Enrolments – manages participant enrolments.                                    *
 *  5. Results – manages race results.                                                 *
 ***************************************************************************************/

using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.ViewModels;
using RaceDay.MVC.Services;
using System.Net;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }


        /*
         * Checks whether the user is logged in.
         */
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }


        /*
         * Checks whether the logged-in user has the organiser role.
         */
        private bool IsOrganiser()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "organiser",
                StringComparison.OrdinalIgnoreCase);
        }


        /*
         * Centralises access control for all organiser actions.
         * Returns a redirect result if access is denied.
         * Returns null when the user is authorised.
         */
        private IActionResult? CheckOrganiserAccess()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (!IsOrganiser())
                return RedirectToAction("AccessDenied", "Home");

            return null;
        }


        /*
         * Retrieves the events belonging to the currently logged-in organiser.
         * Returns both the API status code and the deserialised event list.
         */
        private async Task<(System.Net.HttpStatusCode StatusCode,
                            List<EventApiResponse>? Events)> 
                            GetOrganiserEventsAsync()
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            List<EventApiResponse>? events = null;

            if (response.IsSuccessStatusCode)
            {
                events = await _api.ReadAsync<List<EventApiResponse>>(response);
            }

            return (response.StatusCode, events);
        }


        /*
         * Retrieves the categories for a specific event.
         * Returns an empty list if the API call fails or returns no categories.
         */
        private async Task<List<CategoryApiResponse>> 
                            GetEventCategoriesAsync(int eventId)
        {
            HttpResponseMessage response = await _api
                                                 .SendAsync(
                                                            HttpMethod.Get,
                                                            $"api/events/{eventId}/categories"
                );

            if (!response.IsSuccessStatusCode)
            {
                return new List<CategoryApiResponse>();
            }

            List<CategoryApiResponse>? categories =
                await _api.ReadAsync<List<CategoryApiResponse>>(response);

            if (categories == null)
            {
                return new List<CategoryApiResponse>();
            }

            return categories;
        }


        /*
         * Converts the MVC event form model into the API event request contract.
         * Used by both CreateEvent and EditEvent.
         */
        private EventRequest CreateEventRequest(EventManagementViewModel model)
        {
            return new EventRequest
            {
                EventName = model.EventName,
                Description = model.Description,
                Location = model.Location,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                EventDate = model.EventDate,
                Distance = model.Distance,
                EventType = model.EventType,
                Status = model.Status
            };
        }


        /*
         * Displays the organiser dashboard.
         * Retrieves the organiser's events and calculates enrolment totals.
         */
        public async Task<IActionResult> Index()
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            OrganiserDashboardViewModel dashboard =
                new OrganiserDashboardViewModel();

            var result = await GetOrganiserEventsAsync();

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode))
            {
                dashboard.ErrorMessage =
                    "Unable to retrieve your events. Please try again.";

                return View(dashboard);
            }

            List<EventApiResponse>? events = result.Events;

            if (events == null)
            {
                dashboard.ErrorMessage =
                    "The API returned an unexpected event response.";

                return View(dashboard);
            }

            foreach (EventApiResponse eventItem in events)
            {
                OrganiserEventViewModel eventViewModel =
                    new OrganiserEventViewModel();

                eventViewModel.EventId = eventItem.EventId;
                eventViewModel.EventName = eventItem.EventName;
                eventViewModel.Description = eventItem.Description;
                eventViewModel.Location = eventItem.Location;
                eventViewModel.EventDate = eventItem.EventDate;
                eventViewModel.Distance = eventItem.Distance;
                eventViewModel.EventType = eventItem.EventType;
                eventViewModel.Status = eventItem.Status;

                using var enrolmentsResponse = await _api.SendAsync(
                    HttpMethod.Get,
                    $"api/events/{eventItem.EventId}/enrolments");

                if (enrolmentsResponse.IsSuccessStatusCode)
                {
                    List<EnrolmentApiResponse>? enrolments =
                        await _api.ReadAsync<List<EnrolmentApiResponse>>(
                            enrolmentsResponse);

                    if (enrolments != null)
                    {
                        eventViewModel.EnrolmentCount = enrolments.Count;
                    }
                    else
                    {
                        eventViewModel.EnrolmentCount = null;
                        dashboard.EnrolmentDataAvailable = false;
                    }
                }
                else
                {
                    eventViewModel.EnrolmentCount = null;
                    dashboard.EnrolmentDataAvailable = false;

                    if (enrolmentsResponse.StatusCode ==
                        System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    if (enrolmentsResponse.StatusCode ==
                        System.Net.HttpStatusCode.Forbidden)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }

                dashboard.Events.Add(eventViewModel);
            }

            if (!dashboard.EnrolmentDataAvailable)
            {
                dashboard.ErrorMessage =
                    "Some enrolment counts could not be retrieved.";
            }

            return View(dashboard);
        }


        /*
         * Checks whether an HTTP status code indicates a successful response.
         */
        private bool IsSuccessfulStatusCode(System.Net.HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }


        /*
         * Displays the organiser's event listing.
         */
        public async Task<IActionResult> Events()
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            var result = await GetOrganiserEventsAsync();

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode))
            {
                TempData["ErrorMessage"] =
                    "Unable to retrieve your events.";

                return View(new List<EventApiResponse>());
            }

            if (result.Events == null)
            {
                TempData["ErrorMessage"] =
                    "The API returned an unexpected event response.";

                return View(new List<EventApiResponse>());
            }

            return View(result.Events);
        }


        /*
         * MANAGE CATEGORIES - GET
         * Retrieves and displays the categories belonging to a selected event.
         */
        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            var result = await GetOrganiserEventsAsync();                               // Retrieve the events belonging to this organiser.

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode) || result.Events == null)
                return RedirectToAction(nameof(Events));

            EventApiResponse? selectedEvent = null;                                     // Confirm that the selected event belongs to this organiser.

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    selectedEvent = eventItem;
                    break;
                }
            }

            if (selectedEvent == null)
                return RedirectToAction(nameof(Events));

            List<CategoryApiResponse> categories =
                await GetEventCategoriesAsync(eventId);                                 // Retrieve the categories belonging to the selected event.

            CategoryManagementViewModel model = new CategoryManagementViewModel
            {
                EventId = selectedEvent.EventId,
                EventName = selectedEvent.EventName,
                Categories = categories
            };

            return View(model);
        }

        /*
         * CREATE CATEGORY - POST
         * Validates and sends a new category to the API for the selected event.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            CategoryManagementViewModel model)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            // Confirm that the selected event belongs to this organiser.
            var result = await GetOrganiserEventsAsync();

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode) || result.Events == null)
                return RedirectToAction(nameof(Events));

            bool eventBelongsToOrganiser = false;

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    eventBelongsToOrganiser = true;
                    break;
                }
            }

            if (!eventBelongsToOrganiser)
                return RedirectToAction(nameof(Events));

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(ManageCategories), new { eventId });
            }

            // Prepare the API request.
            CategoryRequest request = new CategoryRequest
            {
                CategoryName = model.CategoryName.Trim()
            };

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                $"api/events/{eventId}/categories",
                request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "The category could not be created. Please try again.";

                return RedirectToAction(nameof(ManageCategories), new { eventId });
            }

            TempData["SuccessMessage"] = "Category created successfully.";

            return RedirectToAction(nameof(ManageCategories), new { eventId });
        }

        /*
         * UPDATE CATEGORY - POST
         * Updates an existing category through the RaceDay API.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(
            int eventId,
            int categoryId,
            CategoryManagementViewModel model)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            // Confirm that the event belongs to this organiser.
            var result = await GetOrganiserEventsAsync();

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode) || result.Events == null)
                return RedirectToAction(nameof(Events));

            bool eventBelongsToOrganiser = false;

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    eventBelongsToOrganiser = true;
                    break;
                }
            }

            if (!eventBelongsToOrganiser)
                return RedirectToAction(nameof(Events));

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter a valid category name.";
                return RedirectToAction(nameof(ManageCategories), new { eventId });
            }

            // Prepare the updated category information.
            CategoryRequest request = new CategoryRequest
            {
                CategoryName = model.CategoryName.Trim()
            };

            using var response = await _api.SendAsync(
                HttpMethod.Put,
                $"api/events/{eventId}/categories/{categoryId}",
                request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "The category could not be updated.";
                return RedirectToAction(nameof(ManageCategories), new { eventId });
            }

            TempData["SuccessMessage"] = "Category updated successfully.";

            return RedirectToAction(nameof(ManageCategories), new { eventId });
        }

        /*
         * DELETE CATEGORY - POST
         * Deletes an existing category through the RaceDay API.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            // Confirm that the event belongs to this organiser.
            var result = await GetOrganiserEventsAsync();

            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!IsSuccessfulStatusCode(result.StatusCode) || result.Events == null)
                return RedirectToAction(nameof(Events));

            bool eventBelongsToOrganiser = false;

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    eventBelongsToOrganiser = true;
                    break;
                }
            }

            if (!eventBelongsToOrganiser)
                return RedirectToAction(nameof(Events));

            // Send the delete request to the API.
            using var response = await _api.SendAsync(
                HttpMethod.Delete,
                $"api/events/{eventId}/categories/{categoryId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "The category could not be deleted.";
                return RedirectToAction(nameof(ManageCategories), new { eventId });
            }

            TempData["SuccessMessage"] = "Category deleted successfully.";

            return RedirectToAction(nameof(ManageCategories), new { eventId });
        }


        /*
         * CREATE EVENT - GET
         * Displays the event creation form.
         */
        [HttpGet]
        public IActionResult CreateEvent()
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            return View(new EventManagementViewModel());
        }


        /*
         * CREATE EVENT - POST
         * Validates the form and sends the new event to the API.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventManagementViewModel model)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            if (!ModelState.IsValid)
                return View(model);

            EventRequest request = CreateEventRequest(model);

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/events",
                request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "The event could not be created. Please check your information and try again.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event created successfully.";

            return RedirectToAction(nameof(Events));
        }


        /*
         * EDIT EVENT - GET
         * Retrieves an existing event and displays its information.
         */
        [HttpGet]
        public async Task<IActionResult> EditEvent(int eventId)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{eventId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Events));

            EventApiResponse? eventItem =
                await _api.ReadAsync<EventApiResponse>(response);

            if (eventItem == null)
                return NotFound();

            EventManagementViewModel model = new EventManagementViewModel
            {
                EventName = eventItem.EventName,
                Description = eventItem.Description,
                Location = eventItem.Location,
                Latitude = eventItem.Latitude,
                Longitude = eventItem.Longitude,
                EventDate = eventItem.EventDate,
                Distance = eventItem.Distance,
                EventType = eventItem.EventType,
                Status = eventItem.Status
            };

            return View(model);
        }


        /*
         * EDIT EVENT - POST
         * Validates the changes and sends an update request to the API.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            int eventId,
            EventManagementViewModel model)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            if (!ModelState.IsValid)
                return View(model);

            EventRequest request = CreateEventRequest(model);

            using var response = await _api.SendAsync(
                HttpMethod.Put,
                $"api/events/{eventId}",
                request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "The event could not be updated. Please try again.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event updated successfully.";

            return RedirectToAction(nameof(Events));
        }


        /*
         * DELETE EVENT - POST
         * Sends a delete request for the selected event to the API.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            IActionResult? accessResult = CheckOrganiserAccess();

            if (accessResult != null)
                return accessResult;

            using var response = await _api.SendAsync(
                HttpMethod.Delete,
                $"api/events/{eventId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Home");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "The event could not be deleted.";

                return RedirectToAction(nameof(Events));
            }

            TempData["SuccessMessage"] = "Event deleted successfully.";

            return RedirectToAction(nameof(Events));
        }

        /*
         * Displays enrolments for a selected event.
         * 1. Verifies that the logged-in user is an organiser.
         * 2. Confirms that the event belongs to the organiser.
         * 3. Retrieves the event's enrolments from the RaceDay API.
         * 4. Passes the enrolment information to the view.
         */
        public async Task<IActionResult> EventEnrollments(int eventId)
        {
            IActionResult accessResult = CheckOrganiserAccess();

            if (accessResult != null)
            {
                return accessResult;
            }

            (HttpStatusCode StatusCode, List<EventApiResponse>? Events) result =
                await GetOrganiserEventsAsync();

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (result.Events == null)
            {
                return RedirectToAction("Events");
            }

            EventApiResponse selectedEvent = null;

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    selectedEvent = eventItem;
                    break;
                }
            }

            if (selectedEvent == null)
            {
                return RedirectToAction("Events");
            }

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{eventId}/enrolments");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve event enrolments.";
                return RedirectToAction("Events");
            }

            List<EnrollmentApiResponse>? enrollments =
                await _api.ReadAsync<List<EnrollmentApiResponse>>(response);

            EventEnrollmentViewModel model = new EventEnrollmentViewModel();

            model.EventId = selectedEvent.EventId;
            model.EventName = selectedEvent.EventName;

            if (enrollments != null)
            {
                model.Enrollments = enrollments;
            }

            return View(model);
        }


        /*
         * Displays the participants who are enrolled in an organiser's event.
         * 1. Verifies organiser access.
         * 2. Confirms the event belongs to the organiser.
         * 3. Retrieves the event enrolments from the API.
         * 4. Displays the participants for results entry.
         */
        public async Task<IActionResult> ResultsEntry(int eventId)
        {
            IActionResult accessResult = CheckOrganiserAccess();
            if (accessResult != null) return accessResult;

            (HttpStatusCode StatusCode, List<EventApiResponse>? Events) result =
                await GetOrganiserEventsAsync();

            if (result.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (result.Events == null)
                return RedirectToAction("Events");

            EventApiResponse selectedEvent = null;

            foreach (EventApiResponse eventItem in result.Events)
            {
                if (eventItem.EventId == eventId)
                {
                    selectedEvent = eventItem;
                    break;
                }
            }

            if (selectedEvent == null)
                return RedirectToAction("Events");

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Get, $"api/events/{eventId}/enrolments");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve event enrolments.";
                return RedirectToAction("Events");
            }

            List<EnrollmentApiResponse>? enrollments =
                await _api.ReadAsync<List<EnrollmentApiResponse>>(response);

            EventEnrollmentViewModel model = new EventEnrollmentViewModel();
            model.EventId = selectedEvent.EventId;
            model.EventName = selectedEvent.EventName;

            if (enrollments != null)
                model.Enrollments = enrollments;

            return View(model);
        }


        /*
         * Records a participant's race result.
         * 1. Verifies organiser access.
         * 2. Validates the submitted times and finishing position.
         * 3. Sends the result to the RaceDay API.
         * 4. Displays the API response to the organiser.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            int eventId,
            int enrolmentId,
            DateTime startTime,
            DateTime finishTime,
            int position)
        {
            IActionResult accessResult = CheckOrganiserAccess();
            if (accessResult != null) return accessResult;

            if (startTime == default || finishTime == default ||
                finishTime <= startTime || position <= 0)
            {
                TempData["ErrorMessage"] =
                    "Enter valid start and finish times and a position greater than zero.";

                return RedirectToAction("ResultsEntry", new { eventId });
            }

            ResultRequest request = new ResultRequest();
            request.StartTime = startTime;
            request.FinishTime = finishTime;
            request.Position = position;

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Post,
                $"api/enrolments/{enrolmentId}/result",
                request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Result recorded successfully.";
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["ErrorMessage"] =
                    "A result has already been recorded for this enrolment.";
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] =
                    "You are not authorised to record results for this event.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to record the result.";
            }

            return RedirectToAction("ResultsEntry", new { eventId });
        }
    }
}
