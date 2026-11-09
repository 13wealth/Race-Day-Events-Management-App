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
         * Redirects to UpcomingEvents to show upcoming events by default.
         */
        public IActionResult Index()
        {
            IActionResult? accessResult = CheckParticipantAccess();

            if (accessResult != null)
                return accessResult;

            return RedirectToAction(nameof(UpcomingEvents));
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


        /*
         * Retrieves all events in which the logged-in participant is enrolled.
         * 1. Checks participant authentication and access.
         * 2. Retrieves the participant's enrolments from the RaceDay API.
         * 3. Retrieves the corresponding event and category details.
         * 4. Displays the participant's enrolment information.
        */
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return Forbid();
            }

            ParticipantMyEventsViewModel model =
                new ParticipantMyEventsViewModel();

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Get, "api/enrolments/mine");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }

            // No enrolments found.
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve your events.";
                return View(model);
            }

            List<ParticipantEnrollmentApiResponse> enrolments =
                await _api.ReadAsync<List<ParticipantEnrollmentApiResponse>>(response);

            foreach (ParticipantEnrollmentApiResponse enrolment in enrolments)
            {
                HttpResponseMessage eventResponse = await _api.SendAsync(
                    HttpMethod.Get, $"api/events/{enrolment.EventId}");

                HttpResponseMessage categoryResponse = await _api.SendAsync(
                    HttpMethod.Get,
                    $"api/events/{enrolment.EventId}/categories/{enrolment.CategoryId}");

                if (!eventResponse.IsSuccessStatusCode ||
                    !categoryResponse.IsSuccessStatusCode)
                {
                    continue;
                }

                EventApiResponse eventDetails =
                    await _api.ReadAsync<EventApiResponse>(eventResponse);

                ParticipantCategoryApiResponse category =
                    await _api.ReadAsync<ParticipantCategoryApiResponse>(categoryResponse);

                ParticipantEnrollmentViewModel enrollmentItem =
                    new ParticipantEnrollmentViewModel();

                enrollmentItem.EnrollmentId = enrolment.EnrollmentId;
                enrollmentItem.EventId = enrolment.EventId;
                enrollmentItem.EventName = eventDetails.EventName;
                enrollmentItem.Location = eventDetails.Location;
                enrollmentItem.EventDate = eventDetails.EventDate;
                enrollmentItem.CategoryName = category.CategoryName;
                enrollmentItem.EntryDate = enrolment.EntryDate;
                enrollmentItem.Status = enrolment.Status;

                model.Enrollments.Add(enrollmentItem);
            }

            return View(model);
        }


        /*
         * Retrieves the details and available categories for a selected event.
         * 1. Checks that the logged-in user is a participant.
         * 2. Retrieves event details from the RaceDay API.
         * 3. Retrieves categories belonging to the selected event.
         * 4. Displays the event details and available categories.
         */
        [HttpGet]
        public async Task<IActionResult> EventDetails(int eventId)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return Forbid();
            }

            HttpResponseMessage eventResponse = await _api.SendAsync(
                HttpMethod.Get, $"api/events/{eventId}");

            if (!eventResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve event details.";
                return RedirectToAction(nameof(UpcomingEvents));
            }

            EventApiResponse eventDetails =
                await _api.ReadAsync<EventApiResponse>(eventResponse);

            HttpResponseMessage categoryResponse = await _api.SendAsync(
                HttpMethod.Get, $"api/events/{eventId}/categories");

            if (!categoryResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve event categories.";
                return RedirectToAction(nameof(UpcomingEvents));
            }

            List<CategoryApiResponse> categories =
                await _api.ReadAsync<List<CategoryApiResponse>>(categoryResponse);

            ParticipantEventDetailsViewModel model =
                new ParticipantEventDetailsViewModel();

            model.Event = eventDetails;
            model.Categories = categories;

            return View(model);
        }


        /*
         * Enrols the logged-in participant in a selected event category.
         * 1. Checks that the logged-in user is a participant.
         * 2. Sends the event and category IDs to the RaceDay API.
         * 3. Handles successful enrolment and API errors.
         * 4. Returns the participant to the event details page.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrolInEvent(
                                                       int eventId, 
                                                       int categoryId
            )
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return Forbid();
            }

            var request = new
            {
                EventId = eventId,
                CategoryId = categoryId
            };

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Post, "api/enrolments", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "You have successfully enrolled in this event.";
                return RedirectToAction(nameof(MyEvents));
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["ErrorMessage"] = "You are already enrolled in this category.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to complete your enrolment.";
            }

            return RedirectToAction(nameof(EventDetails),
                new { eventId = eventId });
        }

        /*
         * Retrieves all recorded race results belonging to the logged-in participant.
         * 1. Checks participant authentication and access.
         * 2. Retrieves the participant's results from the RaceDay API.
         * 3. Handles API errors.
         * 4. Displays the participant's race history and finishing positions.
         */
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return Forbid();
            }

            ParticipantResultsViewModel model =
                new ParticipantResultsViewModel();

            HttpResponseMessage response = await _api.SendAsync(
                HttpMethod.Get, "api/results/mine");

            System.Diagnostics.Debug.WriteLine($"[DEBUG] MyResults response status: {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "Auth");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "You do not have permission to view results.";
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to retrieve your race results. Please try again later.";
                return View(model);
            }

            List<ParticipantResultApiResponse>? results =
                await _api.ReadAsync<List<ParticipantResultApiResponse>>(response);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] MyResults deserialized results count: {results?.Count ?? 0}");

            if (results != null)
            {
                model.Results = results;
            }

            return View(model);
        }
    }
}