
/***************************************************************************************
 *                                    AUTH CONTROLLER                                  *
 * This controller connects the MVC registration and login forms to your existing      *
 *   Part 2 API.                                                                       *
 * It handles three operations:                                                        *
 *  1. Registration – sends the new user's details to the API.                         *
 *  2. Login – authenticates user through and stores the required session information. *
 *  3. Logout – asks the API to end its session and clears the MVC session.            *                                                                 *
 * *************************************************************************************/

using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;                                             // The API client used to communicate with the Part 2 API

    public AuthController(RaceDayApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Display the registration page
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    /// <summary>
    /// Process the registration form
    /// </summary>
    /// <param name="vm">The registration view model</param>
    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)  
        {
            return View(vm);                                                            /* If the model state is invalid, 
                                                                                         * return the view with the current view model to 
                                                                                         * display validation errors
                                                                                         */
        }

        /*
         * Create a RegisterRequest object to send the user's registration details to the API.
         */
        var request = new RegisterRequest
        {
            Username = vm.Username,
            Email = vm.Email,
            Password = vm.Password,
            Role = vm.Role,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Phone = vm.Phone
        };

        /*
         * Send the registration request to the API using the RaceDayApiClient.
         * The API will handle the registration logic and return a response.
         */
        var response = await _api.SendAsync(
                                            HttpMethod.Post,
                                            "api/auth/register",
                                            request
            );

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);

            return View(vm);
        }

        TempData["Success"] = "Registration successful. Please log in.";                // Store a success message in TempData to display on the login page

        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Display the login page
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Process the login form
    /// </summary>
    /// <param name="vm"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var request = new LoginRequest
        {
            Username = vm.Username,
            Password = vm.Password
        };

        var response = await _api.SendAsync(
                                            HttpMethod.Post,
                                            "api/auth/login",
                                            request
            );

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");

            return View(vm);
        }

        var auth = await _api.ReadAsync<AuthResponse>(response);

        if (auth == null)
        {
            ModelState.AddModelError("", "Login response could not be read.");

            return View(vm);
        }

        string? apiCookie = RaceDayApiClient.ExtractSessionCookie(response);            // Retrieve and store the API session cookie

        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            HttpContext.Session.SetString("ApiSessionCookie", apiCookie);
        }

        
        HttpContext.Session.SetInt32("UserId", auth.UserId);                            // Store the logged-in user's information in the MVC session
        HttpContext.Session.SetString("Role", auth.Role);                               // Store the user's role in the MVC session

        HttpContext.Session.SetString(
                                        "DisplayName",
                                        auth.FirstName ?? auth.Email
            );

        if (auth.Role.Equals(
                            "Organiser",
                            StringComparison.OrdinalIgnoreCase
                            )
            )                                                                           // Redirect the user according to their role
        {
            return RedirectToAction("Index", "Organiser");
        }

        return RedirectToAction("Index", "Participant");
    }

    /// <summary>
    /// User logout – clears the MVC session and asks the API to end its session
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.SendAsync(
                                HttpMethod.Post,
                                "api/auth/logout"
            );

        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}


/*
    Part 2 – API	                 | Part 3 – MVC
    Handles authentication logic	 | Handles website authentication workflow
    Communicates with the database	 | Communicates with the API
    Verifies credentials	         | Sends credentials for verification
    Creates API session	             | Preserves API session cookie
    Returns authentication data	     | Uses that data to manage website navigation

    Registration
        1. Browser displays the Register form.
        2. User submits their details.
        3. RegisterVm receives the submitted information.
        4. MVC validates the information and maps it to RegisterRequest.
        5. RaceDayApiClient sends the request to the Part 2 API.
        6. On success, the user is redirected to Login.

    Login
        1. User submits their email and password.
        2. MVC sends a LoginRequest to the API.
        3. The API validates the credentials and creates its session.
        4. MVC reads the AuthResponse and extracts the API session cookie.
        5. MVC stores the cookie, user ID, role and display name in its own session.
        6. User is redirected to the appropriate dashboard.

    Logout
        1. MVC sends a logout request to the API.
        2. MVC clears its session.
        3. User is redirected to the Home page.

    Session value	        | Purpose
    ApiSessionCookie	    | Allows MVC to make authenticated requests to the API.
    UserId	                | Identifies the logged-in user within MVC.
    Role	                | Determines whether the user is an Organiser or Participant.
    DisplayName	            | Provides a name to display in the interface.
*/