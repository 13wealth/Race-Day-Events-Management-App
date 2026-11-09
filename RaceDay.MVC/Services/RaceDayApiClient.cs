
/***************************************************************************************
 *                                  RACE DAY API CLIENT                                *
 * Handles communication between the MVC application and the Part 2 RaceDay API.       *
 * Sends HTTP requests, manages API session cookies, and processes API responses.      *
 * *************************************************************************************/

using System.Net.Http.Json;                                                             // Allows us to work with JSON when sending HTTP requests
using System.Text.Json;                                                                 // Allows us to deserialize JSON responses into C# objects

namespace RaceDay.MVC.Services;

public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;                                       // Allows us to obtain the configured HTTP client.
    private readonly IHttpContextAccessor _httpContextAccessor;                         // Allows us to access the current MVC request, including its session.

    /*
     * Constructor for the RaceDayApiClient class.
     * Initializes the HttpClient using the provided IHttpClientFactory and IHttpContextAccessor.
     */
    public RaceDayApiClient(IHttpClientFactory factory,
                            IHttpContextAccessor accessor)
    {
        _factory = factory;
        _httpContextAccessor = accessor;
    }


    /*
     * Tells C# to ignore differences between uppercase and lowercase letters when matching JSON 
     *  property names to C# property names
     * PropertyNameCaseInsensitive is a built-in .NET option
     */
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };


    /*
     * Asks the factory to create an HTTP client using the configuration we registered in Program.cs 
     *  under the name "RaceDayAPI"
     */
    private HttpClient CreateClient()
    {
        return _factory.CreateClient("RaceDayAPI");
    }


    /*
     * Prepares an HTTP request before we send it to the backend
     * It retrieves two things:
     *  1. method: What type of request we want to make (GET, POST, PUT, DELETE)
     *  2. url: Which API address we want to access
     * Important: This method does not log the user in. It simply carries their existing 
     *  session information along with the request
     */
    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);                             // Create a new HTTP request message with the specified method and URL

        /*
         * Retrieves the API's session cookie that was previously stored in the MVC application's session
         *  _httpContextAccessor	Gives us access to the current HTTP context.
            .HttpContext	        Gets the current HTTP request context.
            .Session	            Accesses the current user's MVC session.
            .GetString(...)	        Retrieves a stored string using its key.
         */
        string apiCookie = _httpContextAccessor
                           .HttpContext
                           .Session
                           .GetString("ApiSessionCookie");                             

        if (!string.IsNullOrWhiteSpace(apiCookie))                                     // Checks whether MVC has an API session cookie stored in its session
        {
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);              // If one exists, it attaches that cookie to the outgoing request
        }

        return request;
    }


    /*
     * Sends the actual HTTP request to the backend API.
     * Async allows the method to perfomr asynchronous operations
     * HttpResponseMessage is built-in .NET class that represents the 
     *  response received from an HTTP request
     */
    public async Task<HttpResponseMessage> SendAsync(
                                                        HttpMethod method,
                                                        string url,
                                                        object? body = null
        )
    {
        var client = CreateClient();                                                    // Calls CreateClient() to obtain the configured HTTP client
        using var request = CreateRequest(method, url);                                 // Calls CreateRequest() to prepare the request
        
        if (body != null)                                                               // Checks whether data was supplied in body
        {
            request.Content = JsonContent.Create(body);                                 // If data exists, converts it into JSON (Serialize)
        }

        return await client.SendAsync(request);                                         // Sends the request to the API
    }


    /*
     * Takes the response received from the API and converts its JSON content into a C# object.
     * Returns the deserialized object or null if the response is not successful.
     */
    public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)                                              // Checks if the HTTP response indicates a successful status code (2xx)
        {
            return default;
        }

        string json = await response.Content.ReadAsStringAsync();                       // Reads the response content as a string (JSON format)

        return JsonSerializer.Deserialize<T>(json, JsonOptions);                        // Deserializes the JSON string into a C# object using JsonOptions
    }


    /*
     * Extracts the session cookie value from the HTTP response headers.
     * string? - Returns the session cookie value or null if not found.
     * HttpResponseMessage response - receives the API response
     */
    public static string? ExtractSessionCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))               // Checks if the response headers contain a "Set-Cookie" header
        {
            return null;
        }


        /*
         * Searches for the session cookie in the "Set-Cookie" header values.
         * FirstOrDefault() returns the first matching value.
         * v represents the current cookie value being checked.
         * StartsWith() checks whether it begins with .AspNetCore.Session=.
         * StringComparison.OrdinalIgnoreCase makes the comparison case-insensitive.
         */
        string? sessionHeader = values
                                .FirstOrDefault(v =>
                                                    v.StartsWith(
                                                                ".AspNetCore.Session=", 
                                                                StringComparison.OrdinalIgnoreCase
                                                                )
                );

        return sessionHeader?.Split(';', 2)[0];                                         /*
                                                                                         * Split(';', 2) separates the string at the first semicolon into two parts.
                                                                                         * [0] selects the first part.
                                                                                         * The ?. operator ensures that if sessionHeader is 
                                                                                         *   null, the method returns null instead of causing an error.
                                                                                         */
    }
}


/****************************************************************************************************************************
 * DESERIALISATION                                                                                                          *
 * When our MVC application requests information from the API, the API usually returns JSON                                 *
 * Our MVC application needs to convert that JSON into a C# object.                                                         *           
 * This process is called deserialization.                                                                                  *
 *                                                                                                                          *
 * JsonOptions is a set of settings that control how the JSON is read and converted into C# objects.                        *                                                                        *
 * ReadAsync method uses these settings to ensure that the JSON is deserialized correctly, even if the property names in    *                                  * 
 *  the JSON do not match the property names in the C# class.                                                               *
 *                                                                                                                          *
 * FLOW                                                                                                                     *
 *  1. MVC sends login details to the API.                                                                                  *
 *  2. The API authenticates the user.                                                                                      *
 *  3. The API returns a session cookie.                                                                                    *
 *  4. MVC extracts that cookie.                                                                                            *
 *  5. MVC stores it in its own session.                                                                                    *
 *  6. Future API requests use CreateRequest() to attach that cookie.                                                       *     
 *                                                                                                                          *
 * EXTRA NOTES                                                                                                              *
 *  A factory is something that creates objects for us.                                                                     *
 *  IHttpClientFactory is a built-in .NET service that creates and manages HttpClient objects.                              *
 *  Instead of manually creating an HttpClient every time, we ask the factory to create one                                 *                
 * **************************************************************************************************************************/