using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using DotNetEnv;
using System;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();  


builder.Services.AddControllers();

// Add DbContext with MySQL configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 32)) 
    ));

// Retrieve the ClientId and ClientSecret from environment variables or appsettings.json
var clientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId") ??
               builder.Configuration["Authentication:Google:ClientId"];

var clientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret") ??
                   builder.Configuration["Authentication:Google:ClientSecret"];

// Log to verify if the ClientId and ClientSecret are correctly loaded
Console.WriteLine($"ClientId: {clientId}");
Console.WriteLine($"ClientSecret: {clientSecret}");

// Check if ClientId and ClientSecret are null or empty
if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
{
    throw new InvalidOperationException("Google ClientId or ClientSecret is not set in the environment or configuration.");
}

// Add Authentication with Google and Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;  // Use cookies for authentication
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;        // Use cookies for signing in
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;                   // Use Google for challenges (login redirects)
})
.AddCookie()  // Cookie authentication
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = clientId;
    googleOptions.ClientSecret = clientSecret;
    googleOptions.SaveTokens = true;
    googleOptions.CallbackPath = "/signin-google";  // redirect URI matches this in the Google Developer Console
});


builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials for login sessions (like cookies)
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger at the root (http://localhost:5031/)
    });
}

app.UseCors("AllowAllOrigins");


app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();


app.MapControllers();

app.Run();
