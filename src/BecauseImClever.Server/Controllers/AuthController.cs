namespace BecauseImClever.Server.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller for handling authentication operations.
    /// </summary>
    [Route("auth")]
    public class AuthController : Controller
    {
        private const string AdminGroupName = "becauseimclever-admins";
        private const string GuestWriterGroupName = "becauseimclever-writers";

        /// <summary>
        /// Initiates the OpenID Connect login flow.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after successful login.</param>
        /// <returns>A challenge result that redirects to the identity provider.</returns>
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/",
            };

            return this.Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Signs out the user from both the application and the identity provider.
        /// </summary>
        /// <returns>A sign-out result that ends the session.</returns>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await Task.CompletedTask; // Placeholder for any async operations

            var properties = new AuthenticationProperties
            {
                RedirectUri = "/",
            };

            return this.SignOut(
                properties,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Gets the current user's information.
        /// </summary>
        /// <returns>
        /// An OK result with user information if authenticated;
        /// otherwise, an Unauthorized result.
        /// </returns>
        [HttpGet("user")]
        public IActionResult GetCurrentUser()
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                return this.Unauthorized();
            }

            var claims = this.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var groups = this.User.Claims.Where(c => c.Type == "groups").Select(c => c.Value).ToList();
            var isAdmin = groups.Contains(AdminGroupName);
            var isGuestWriter = groups.Contains(GuestWriterGroupName);

            var userInfo = new
            {
                Name = this.User.Identity?.Name ?? this.User.FindFirst(ClaimTypes.Name)?.Value,
                Email = this.User.FindFirst(ClaimTypes.Email)?.Value,
                IsAdmin = isAdmin,
                IsGuestWriter = isGuestWriter,
                CanManagePosts = isAdmin || isGuestWriter,
                Claims = claims,
            };

            return this.Ok(userInfo);
        }
    }
}
