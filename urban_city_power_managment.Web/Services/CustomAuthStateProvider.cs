using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Custom AuthenticationStateProvider for Blazor Server that maintains auth state
    /// across SignalR reconnections using a scoped service pattern.
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public CustomAuthStateProvider(ILogger<CustomAuthStateProvider> logger)
        {
            _logger = logger;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        /// <summary>
        /// Marks the user as authenticated and notifies all subscribers
        /// </summary>
        public void MarkUserAsAuthenticated(UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            _currentUser = new ClaimsPrincipal(identity);

            _logger.LogInformation("User {UserId} marked as authenticated", user.Id);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Marks the user as logged out and notifies all subscribers
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            _logger.LogInformation("User marked as logged out");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Gets the current user ID if authenticated
        /// </summary>
        public int? GetCurrentUserId()
        {
            var userIdClaim = _currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? _currentUser.FindFirst("UserId")?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;
    }
}
