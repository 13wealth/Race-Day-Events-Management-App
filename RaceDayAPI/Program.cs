using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ConfigureSwagger);
builder.Services.AddDbContext<RaceDayContext>(ConfigureDatabase);
builder.Services.AddScoped<IPasswordHasher<RaceDayUser>, PasswordHasher<RaceDayUser>>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(ConfigureSession);

var app = builder.Build();

if (app.Environment.IsDevelopment())                                                    // Check if the app is running in development mode
{
    app.UseSwagger();                                                                   // Enable Swagger middleware to generate the OpenAPI specification
    app.UseSwaggerUI();                                                                 // Enable Swagger UI middleware to serve the Swagger UI interface
}

app.UseHttpsRedirection();                                                              // Enable middleware to redirect HTTP requests to HTTPS
app.UseSession();                                                                       // Enable middleware to manage user sessions
app.UseAuthorization();                                                                 // Enable middleware to handle authorization for incoming requests
app.MapControllers();                                                                   // Map controller routes to the corresponding endpoints
app.Run();                                                                              // Start the application and listen for incoming requests

/*
 * Helper methods for configuring services
 * Configure the database context to use SQL Server with the 
 *  connection string from appsettings.json
 */
void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
}

/*
 * Configure session options for the application
 */
void ConfigureSession(SessionOptions options)
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);                                     // Set the session timeout to 30 minutes 
    options.Cookie.HttpOnly = true;                                                     // Set the session cookie to be accessible only via HTTP requests
    options.Cookie.IsEssential = true;                                                  // Mark the session cookie as essential for the application to function properly
}

/*
 * Tells Swagger to read the XML comments written above each endpoint
 */
void ConfigureSwagger(SwaggerGenOptions options)
{
    string xmlPath = Path.Combine(AppContext.BaseDirectory, "RaceDayAPI.xml");
    options.IncludeXmlComments(xmlPath);
}