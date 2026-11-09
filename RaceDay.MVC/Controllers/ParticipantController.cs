
/***************************************************************************************
 *                               PARTICIPANT CONTROLLER                                *
 * This controller manages the participant section of the RaceDay MVC application.     *
 * It handles operations such as:                                                      *
 *  1. Dashboard – displays available race events.                                     *
 *  2. Access Control – ensures only authenticated participants access these pages.    *
 *  3. Enrolments – allows participants to enrol in events and view their enrolments.  *
 *  4. Results – displays participant race history and results.                        *
 *  5. Profile – manages participant profile information.                              *
 ***************************************************************************************/

using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        /*
         * Checks if the user is logged in by verifying the presence of a "UserId" in the session.
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
         * Checks if the logged-in user has the role of "participant".
         * Returns true if the user is a participant, false otherwise.
         */
        private bool IsParticipant()
        {
            return string.Equals(
                                HttpContext.Session.GetString("Role"),
                                "participant",
                                StringComparison.OrdinalIgnoreCase
            );
        }


        /*
         * Displays the participant dashboard if the user is logged in and has the "participant" role.
         * If the user is not logged in, they are redirected to the login page.
         * If the user is logged in but does not have the "participant" role, they are redirected to an access denied page.
         */
        public IActionResult Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsParticipant())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View();
        }
    }
}