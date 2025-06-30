using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CsvProcessorApp.Models;
using CsvProcessorApp.Data;
using System.Linq;

namespace CsvProcessorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Check credentials
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username && u.Password == request.Password);

            if (user != null)
            {
                // Create user claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };

                // Create identity and principal
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Sign the user in using the same scheme as configured in Program.cs
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Ok(new { success = true });
            }

            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true });
        }
    }
}
