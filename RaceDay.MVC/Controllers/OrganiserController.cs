
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
using RaceDay.Mvc.ViewModels;
using RaceDay.MVC.Services;
using RaceDay.Contracts;

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
                            List<EventApiResponse>? Events)> GetOrganiserEventsAsync()
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
    }
}
