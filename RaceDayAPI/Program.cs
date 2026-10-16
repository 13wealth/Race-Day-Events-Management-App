using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<RaceDayContext>(ConfigureDatabase);                       // Configure the database connection for RaceDayContext

var app = builder.Build();

/*
 * The following code configures the HTTP request pipeline.
 * It enables OpenAPI documentation in development environment, enforces HTTPS redirection,
 * and sets up authorization and controller mapping.
 */
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();                                                              // Enforces HTTPS redirection for all incoming requests
app.UseAuthorization();                                                                 // Enables authorization middleware for the application
app.MapControllers();                                                                   // Maps controller endpoints to the application
app.Run();                                                                              // Starts the application and begins listening for incoming HTTP requests

/*
 * ConfigureDatabase method is used to configure the database connection.
 * It uses the connection string named "DefaultConnection" from the appsettings.json file.
 */
void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseSqlServer(
                          builder
                          .Configuration
                          .GetConnectionString("DefaultConnection")
    );
}