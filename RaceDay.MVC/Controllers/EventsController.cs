
/*
 * EVENTS CONTROLLER
 * Handles event-related image requests from the MVC application.
 *
 * Operations:
 * 1. Retrieves event banner images through the RaceDay API.
 * 2. Returns the image to the browser for display.
 */

using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using System.Net;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiClient _api;

        public EventsController(RaceDayApiClient api)
        {
            _api = api;
        }

        /*
         * Retrieves an event banner through the API.
         * The API session cookie is forwarded by RaceDayApiClient.
         */
        [HttpGet]
        public async Task<IActionResult> Banner(int eventId)
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{eventId}/banner");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return Unauthorized();

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            byte[] image =
                await response.Content.ReadAsByteArrayAsync();

            string contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            return File(image, contentType);
        }
    }
}
