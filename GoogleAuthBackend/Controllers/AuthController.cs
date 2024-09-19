using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // Step 1: Start Google OAuth login process
    [HttpGet("login-google")]
    public IActionResult GoogleLogin()
    {
        // Redirect the user to the Google authentication page
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // Step 2: Handle the response from Google after login
    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (result?.Principal != null)
        {
            // Extract Google user information from the authentication result
            var email = result.Principal.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = result.Principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;  // GoogleId is stored in NameIdentifier ("sub")

            if (string.IsNullOrEmpty(googleId))
            {
                return BadRequest("GoogleId is missing from the authentication response.");
            }

            // Step 3: Check if the user exists in the database, or create a new one
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user == null)
            {
                // Ensure email and name are available before creating the user
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                {
                    return BadRequest("Required user information (email or name) is missing from the Google authentication response.");
                }

                // Create new user if not already existing
                user = new User { Email = email, Name = name, GoogleId = googleId };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Step 4: Redirect to the frontend profile page with URL-encoded user info
            var redirectUrl = $"http://localhost:5173/profile?email={Uri.EscapeDataString(user.Email)}&name={Uri.EscapeDataString(user.Name)}";
            return Redirect(redirectUrl);
        }

        return BadRequest("Error authenticating with Google.");
    }

    // Step 5: Retrieve the user's profile if authenticated
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        if (User.Identity.IsAuthenticated)
        {
            var googleId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(googleId))
            {
                return Unauthorized("GoogleId not found in the user claims.");
            }

            // Look up the user by their GoogleId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user != null)
            {
                return Ok(new { user.Email, user.Name });
            }
        }

        return Unauthorized();
    }
}
