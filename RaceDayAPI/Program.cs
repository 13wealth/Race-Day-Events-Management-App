using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using UserModel = RaceDayAPI.Models.RaceDayUser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RaceDayContext>(ConfigureDatabase);   // Database connection for RaceDayContext

// Password hashing (requirement 2e): used by AuthController
builder.Services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();

// Session management (requirement 2b): stores UserId and Role between requests
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

/*
 * Development-only documentation: OpenAPI JSON and the Swagger UI test page.
 */
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();    // Redirects HTTP requests to HTTPS
app.UseSession();             // Must come before MapControllers, or sessions won't work
app.UseAuthorization();       // Authorization middleware
app.MapControllers();         // Maps controller endpoints
app.Run();                    // Starts the application

/*
 * Configures the database connection using "DefaultConnection" from appsettings.json.
 */
void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseSqlServer(
                          builder
                          .Configuration
                          .GetConnectionString("DefaultConnection")
    );
}