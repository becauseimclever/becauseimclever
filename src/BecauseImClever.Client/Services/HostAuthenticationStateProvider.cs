namespace BecauseImClever.Client.Services
{
    using System.Net.Http.Json;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;

    /// <summary>
    /// Provides authentication state by querying the server-side authentication endpoint.
    /// </summary>
    public class HostAuthenticationStateProvider : AuthenticationStateProvider
    {
        private const string AuthUserEndpoint = "auth/user";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making requests to the server.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is null.</exception>
        public HostAuthenticationStateProvider(HttpClient httpClient)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var response = await this.httpClient.GetAsync(AuthUserEndpoint);

                if (!response.IsSuccessStatusCode)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();

                if (userInfo == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = new List<Claim>();

                if (!string.IsNullOrEmpty(userInfo.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, userInfo.Name));
                }

                if (!string.IsNullOrEmpty(userInfo.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
                }

                if (userInfo.IsAdmin)
                {
                    claims.Add(new Claim("groups", "becauseimclever-admins"));
                }

                // Add additional claims from the server
                if (userInfo.Claims != null)
                {
                    foreach (var claim in userInfo.Claims)
                    {
                        // Avoid duplicating claims we've already added
                        if (claim.Type != ClaimTypes.Name && claim.Type != ClaimTypes.Email && claim.Type != "groups")
                        {
                            claims.Add(new Claim(claim.Type, claim.Value));
                        }
                    }
                }

                var identity = new ClaimsIdentity(claims, "ServerAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (HttpRequestException)
            {
                // If we can't reach the server, return an unauthenticated state
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        /// <summary>
        /// Notifies the authentication state has changed.
        /// Call this method after login or logout to update the UI.
        /// </summary>
        public void NotifyAuthenticationStateChanged()
        {
            this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Represents user information returned from the server.
        /// </summary>
        private sealed class UserInfo
        {
            /// <summary>
            /// Gets or sets the user's display name.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the user's email address.
            /// </summary>
            public string? Email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the user is an admin.
            /// </summary>
            public bool IsAdmin { get; set; }

            /// <summary>
            /// Gets or sets the user's claims.
            /// </summary>
            public List<ClaimInfo>? Claims { get; set; }
        }

        /// <summary>
        /// Represents a single claim.
        /// </summary>
        private sealed class ClaimInfo
        {
            /// <summary>
            /// Gets or sets the claim type.
            /// </summary>
            public string Type { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the claim value.
            /// </summary>
            public string Value { get; set; } = string.Empty;
        }
    }
}
