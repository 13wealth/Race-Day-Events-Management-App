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

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
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
         * If the user is not logged in, redirect to the login page.
         * If the user is not an organiser, redirect to the access denied page.
         * Otherwise, return the organiser dashboard view.
         */
        public IActionResult Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsOrganiser())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View();
        }
    }
}