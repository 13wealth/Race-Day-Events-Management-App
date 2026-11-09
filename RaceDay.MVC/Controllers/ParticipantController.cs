/***************************************************************************************
 *                               PARTICIPANT CONTROLLER                                *
 * This controller manages the participant section of the RaceDay MVC application.     *
 * It handles operations such as:                                                      *
 *  1. Dashboard – displays the participant dashboard.                                 *
 *  2. Access Control – ensures only authenticated participants access these pages.    *
 *  3. Upcoming Events – retrieves and filters available race events.                  *
 *  4. Enrolments – allows participants to enrol in events and view their enrolments.  *
 *  5. Results – displays participant race history and results.                        *
 *  6. Profile – manages participant profile information.                              *
 ***************************************************************************************/

using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.Mvc.ViewModels;
using System.Net;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
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
         * Checks whether the logged-in user has the participant role.
         */
        private bool IsParticipant()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "participant",
                StringComparison.OrdinalIgnoreCase);
        }

        /*
         * Checks participant access before displaying protected pages.
         */
        private IActionResult? CheckParticipantAccess()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return null;
        }

        /*
         * Displays the participant dashboard.
         */
        public IActionResult Index()
        {
            IActionResult? accessResult = CheckParticipantAccess();

            if (accessResult != null)
                return accessResult;

            return View();
        }

        /*
         * Displays upcoming race events.
         * 1. Verifies participant access.
         * 2. Retrieves events from the RaceDay API.
         * 3. Excludes events scheduled in the past.
         * 4. Applies search, event type and location filters.
         * 5. Passes the filtered events to the view.
         */
        public async Task<IActionResult> UpcomingEvents(
            string? searchTerm,
            string? eventType,
            string? location)
        {
            IActionResult? accessResult = CheckParticipantAccess();

            if (accessResult != null)
                return accessResult;

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Get, "api/events");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Auth");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve available events.";
                return View(new ParticipantEventsViewModel());
            }

            List<EventApiResponse>? events =
                await _api.ReadAsync<List<EventApiResponse>>(response);

            ParticipantEventsViewModel model =
                new ParticipantEventsViewModel();

            if (events != null)
            {
                foreach (EventApiResponse eventItem in events)
                {
                    if (eventItem.EventDate.Date >= DateTime.Today)
                    {
                        model.Events.Add(eventItem);
                    }
                }
            }

            model.SearchTerm = searchTerm ?? "";
            model.SelectedEventType = eventType ?? "";
            model.SelectedLocation = location ?? "";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model.Events = model.Events.Where(e =>
                    e.EventName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (e.Description != null &&
                     e.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    e.Location.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                model.Events = model.Events.Where(e =>
                    e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                model.Events = model.Events.Where(e =>
                    e.Location.Equals(location, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            model.EventTypes = model.Events
                .Select(e => e.EventType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(e => e)
                .ToList();

            model.Locations = model.Events
                .Select(e => e.Location)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(e => e)
                .ToList();

            return View(model);
        }
    }
}