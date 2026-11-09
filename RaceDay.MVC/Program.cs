using RaceDay.MVC.Services;


/***************************************************************************************
 *                                SERVICE REGISTRATION                                 *
 * Registers and configures the services the application will use in the MVC framework.*
 * *************************************************************************************/

var builder = WebApplication.CreateBuilder(args);                                       // Initializes a new instance of the WebApplicationBuilder class
builder.Services.AddControllersWithViews();                                             // Add services to the container.
builder.Services.AddDistributedMemoryCache();                                           // Configure session storage to use in-memory cache

/*
 * Middleware for setting up session support in the application.
 * Responsible for managing user sessions, 
 *   allowing data to be stored across multiple requests.
 * Uses the object "options" to configure the session settings
 */
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);                                     // If a session is inactive for 30 minutes, it will expire
    options.Cookie.HttpOnly = true;                                                     // Set the session cookie to be accessible only via HTTP
    options.Cookie.IsEssential = true;                                                  // Mark the session cookie as essential for the application
    options.Cookie.SameSite = SameSiteMode.Lax;                                         // Set SameSite to Lax to allow cross-site cookies
});

builder.Services.AddHttpContextAccessor();	                                            // Adds a service to provide access to the current HTTP context

/*
 * Registers and configures a named HTTP client called "RaceDayAPI".
 * This client is configured with the base URL of the Part 2 backend API.
 */
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]                       // Get the base URL from the configuration
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");          /* Checks whether the expression on its left is null.
                                                                                         * In this case, it checks builder.Configuration["ApiSettings:BaseUrl"]
                                                                                         */
    client.BaseAddress = new Uri(baseUrl);                                              // Set the base address for the HTTP client to the specified base URL
});

/*
 * Registers RaceDayApiClient as a scoped service.
 * This class uses IHttpClientFactory to obtain the configured
 * "RaceDayAPI" HTTP client and communicate with the backend.
 */
builder.Services.AddScoped<RaceDayApiClient>();                                        



/***************************************************************************************
 *                      BUILD AND APPLICATION CONFIGURATION                            *
 * Configures how the application handles incoming HTTP requests using middleware      *
 * *************************************************************************************/

var app = builder.Build();

/*
 * Configures the middleware pipeline for handling HTTP requests.
 * Checks if the application is running in a development environment.
 */
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();                                                                       // Identifies which endpoint should handle the request
app.UseSession();                                                                       // Activates session management middleware to handle user sessions
app.UseAuthorization();                                                                 // Handles middleware authorization requirements 
app.MapStaticAssets(); 

/*
 * Configures the routing for the application.
 * Defines a default route pattern that maps incoming requests to the appropriate controller and action.
 * The default route is set to the "Home" controller and the "Index" action, with an optional "id" parameter.
 */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();                                                                              // Starts the application and begins listening for incoming HTTP requests
