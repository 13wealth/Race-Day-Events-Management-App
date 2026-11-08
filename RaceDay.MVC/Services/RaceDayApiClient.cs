/***************************************************************************************
 *                                  RACE DAY API CLIENT                                *
 * Handles communication between the MVC application and the Part 2 RaceDay API.       *
 * Sends HTTP requests, manages API session cookies, and processes API responses.      *
 * *************************************************************************************/

using System.Net.Http.Json;                                                             // Allows us to work with JSON when sending HTTP requests
using System.Text.Json;                                                                 // Allows us to deserialize JSON responses into C# objects

namespace RaceDay.Mvc.Services;

public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;                                       // Factory for creating HttpClient instances
    private readonly IHttpContextAccessor _httpContextAccessor;                         // Accessor for accessing the current HTTP context, including session and cookies

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
     * Creates and stores a set of JSON deserialization settings
     * Configures how json is read and converted into C# objects
     */
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };


    /*
     * Creates and returns a new HttpClient instance configured for the RaceDay API.
     * Returns an HTTP client object and takes no parameters.
     * Uses the IHttpClientFactory to create the client with the name "RaceDayAPI".
     */
    private HttpClient CreateClient()
    {
        return _factory.CreateClient("RaceDayAPI");
    }

    /*
     * Retrieves the API cookie from MVC session and attaches it to an outgoing API request. 
     * We are not creating or authenticating the API session here.
     */
    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);                              // Create a new HTTP request message with the specified method and URL
        string apiCookie = _httpContextAccessor
                            .HttpContext
                            .Session
                            .GetString("ApiSessionCookie");                             // Retrieves the API session cookie from the current HTTP session

        if (!string.IsNullOrWhiteSpace(apiCookie))                                      // Checks if the API session cookie is not null or whitespace
        {
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);               // Attach the cookie to the request header
        }

        return request;
    }

    /*
     * Responsible for sending an HTTP request to the RaceDay API.
     * Async allows the method to perfomr asynchronous operations
     */
    public async Task<HttpResponseMessage> SendAsync(
                                                        HttpMethod method,
                                                        string url,
                                                        object? body = null
        )
    {
        var client = CreateClient();                                                    // Create an HTTP client instance for the RaceDay API
        using var request = CreateRequest(method, url);                                 // Create an HTTP request message with the specified method and URL

        if (body != null)                                                               // Check if the body exists
        {
            request.Content = JsonContent.Create(body);                                 // Serialize the body object to JSON and set it as the request content
        }

        return await client.SendAsync(request);                                         // This is where the actual HTTP request is sent to the API
    }

    /*
     * Reads the HTTP response from the RaceDay API and deserializes it into a C# object of type T.
     * Returns the deserialized object or null if the response is not successful.
     */
    public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)                                              // Checks if the HTTP response indicates a successful status code (2xx)
        {
            return default;
        }

        string json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /*
     * Extracts the session cookie from the HTTP response headers.
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


/********************************************************************************************
 * DESERIALISATION                                                                          *
 * When our MVC application requests information from the API, the API usually returns JSON *
 * Our MVC application needs to convert that JSON into a C# object.                         *           
 * This process is called deserialization.                                                  *
 *                                                                                          *
 * JsonOptions is a set of settings that control how the JSON is read and converted         *
 *  into C# objects.                                                                        *
 * ReadAsync method uses these settings to ensure that the JSON is deserialized correctly,  * 
 *   even if the property names in the JSON do not match the property names in the C# class.*
 * ******************************************************************************************/