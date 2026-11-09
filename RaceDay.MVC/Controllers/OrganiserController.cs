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
         * Check if the user is logged in by verifying if the "UserId" session variable exists.
         * Returns true if the user is logged in, false otherwise.
         */
        private bool IsLoggedIn()
        {
            return HttpContext
                   .Session
                   .GetInt32("UserId")
                   .HasValue; 
        }


        /*
         * Check if the logged-in user has the role of "organiser" by comparing the "Role" session variable.
         * Returns true if the user is an organiser, false otherwise.
         */
        private bool IsOrganiser()
        {
            return string.Equals(
                                HttpContext.Session.GetString("Role"),
                                "organiser",
                                StringComparison.OrdinalIgnoreCase
            );
        }


        /*
         * Displays the organiser dashboard.
         * Retrieves the organiser's events from the API.
         * Retrieves enrolments for each event and calculates dashboard totals.
         */
        public async Task<IActionResult> Index()
        {
            OrganiserDashboardViewModel dashboard = new OrganiserDashboardViewModel();  // Create a new instance of the dashboard view model.

            if (!IsLoggedIn())                                                          // Check whether the user is logged in.
            {
                return RedirectToAction("Login", "Auth");                               // Redirect to the login page if not logged in.
            }
       
            if (!IsOrganiser())                                                         // Check whether the logged-in user is an organiser.
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            /*
             * Retrieve the organiser's events from the API.
             * If the API call is successful, process each event to retrieve its enrolment count.
             * If the API call fails, display an error message on the dashboard.
             */
            using var eventsResponse = await _api.SendAsync(
                                                                    HttpMethod.Get,
                                                                    "api/events/mine"
                );

            if (eventsResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");                               // Redirect to the login page if the API returns an unauthorized status code.
            }

            if (!eventsResponse.IsSuccessStatusCode)
            {
                dashboard.ErrorMessage =
                    "Unable to retrieve your events. Please try again.";

                return View(dashboard);                                                 // Return the dashboard view with an error message if the API call fails.
            }

            /*
             * Read the list of events from the API response.
             * If the API response is null, display an error message on the dashboard.
             */
            List<EventApiResponse>? events = 
                await _api
                      .ReadAsync<List<EventApiResponse>>(
                            eventsResponse
                );

            if (events == null)
            {
                dashboard.ErrorMessage =
                    "The API returned an unexpected event response.";

                return View(dashboard);
            }

            /*
             * Process each event to retrieve its enrolment count.
             * If the enrolment API call fails, display an error message on the dashboard.
             */
            foreach (EventApiResponse eventItem in events)
            {
                OrganiserEventViewModel eventViewModel = new OrganiserEventViewModel();

                eventViewModel.EventId = eventItem.EventId;
                eventViewModel.EventName = eventItem.EventName;
                eventViewModel.Description = eventItem.Description;
                eventViewModel.Location = eventItem.Location;
                eventViewModel.EventDate = eventItem.EventDate;
                eventViewModel.Distance = eventItem.Distance;
                eventViewModel.EventType = eventItem.EventType;
                eventViewModel.Status = eventItem.Status;

                /*
                 * Retrieve the enrolments for the current event from the API.
                 */
                using var enrolmentsResponse = 
                    await _api
                          .SendAsync(
                                     HttpMethod.Get,
                                     $"api/events/{eventItem.EventId}/enrolments"
                    );

                /*
                 * If the enrolment API call is successful, read the list of enrolments 
                 * If the enrolment API call fails, set the enrolment count to null 
                 */
                if (enrolmentsResponse.IsSuccessStatusCode) 
                {
                    List<EnrolmentApiResponse>? enrolments =
                        await _api
                              .ReadAsync<List<EnrolmentApiResponse>>(
                                    enrolmentsResponse
                        );

                    /*
                     * If the enrolments list is NOT null, set the enrolment count in the event view model.
                     * If the enrolments list is null, set the enrolment count to null 
                     */
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

                    /*
                     * If the enrolment API call fails with an Unauthorized status code,
                     * redirect the user to Access Denied page.
                     */
                    if (enrolmentsResponse.StatusCode ==
                        System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    /*
                     * If the enrolment API call fails with a Forbidden status code,
                     * redirect the user to the Access Denied page.
                     */
                    if (enrolmentsResponse.StatusCode ==
                        System.Net.HttpStatusCode.Forbidden)
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }

                dashboard.Events.Add(eventViewModel);                                   // Add the event view model to the dashboard's list of events.
            }

            /*
             * If any enrolment data could not be retrieved, set an error message on the dashboard.
             */
            if (!dashboard.EnrolmentDataAvailable)
            {
                dashboard.ErrorMessage =
                    "Some enrolment counts could not be retrieved.";
            }

            return View(dashboard);
        }
    }
}