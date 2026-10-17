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

// Password hashing (requirement 2e)
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Sessions (requirement 2b)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(ConfigureSession);

var app = builder.Build();

// Swagger page at /swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();        // must come before MapControllers
app.UseAuthorization();
app.MapControllers();
app.Run();

void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
}

void ConfigureSession(SessionOptions options)
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}

// Tells Swagger to read the XML comments written above each endpoint
void ConfigureSwagger(SwaggerGenOptions options)
{
    string xmlPath = Path.Combine(AppContext.BaseDirectory, "RaceDayAPI.xml");
    options.IncludeXmlComments(xmlPath);
}