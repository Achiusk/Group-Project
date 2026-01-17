using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;
using System.Text.Json;

namespace urban_city_power_managment.Web.Services
{
  /// <summary>
    /// Authentication service for user registration and login
    /// Uses BCrypt for secure password hashing and sessions for state
    /// </summary>
public interface IAuthService
    {
        Task<(bool Success, string Message, UserAccount? User)> RegisterAsync(RegistrationModel model);
        Task<(bool Success, string Message, UserAccount? User)> LoginAsync(LoginModel model);
        Task LogoutAsync();
        Task<UserAccount?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> EmailExistsAsync(string email);
        Task<UserAccount?> GetUserByIdAsync(int userId);
        Task<UserAccount?> GetUserByEmailAsync(string email);
        Task<bool> UpdateLastLoginAsync(int userId);
   string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
    private readonly EnergyDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;
        private readonly INetbeheerderService _netbeheerderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // BCrypt work factor (higher = more secure but slower)
        private const int BCRYPT_WORK_FACTOR = 12;
        private const string USER_SESSION_KEY = "CurrentUser";

        public AuthService(
            EnergyDbContext dbContext,
  ILogger<AuthService> logger,
         INetbeheerderService netbeheerderService,
         IHttpContextAccessor httpContextAccessor)
     {
       _dbContext = dbContext;
     _logger = logger;
          _netbeheerderService = netbeheerderService;
          _httpContextAccessor = httpContextAccessor;
        }

   public async Task<(bool Success, string Message, UserAccount? User)> RegisterAsync(RegistrationModel model)
        {
            try
            {
// Validate age (must be 18+)
    if (!model.IsAtLeast18YearsOld())
         {
         return (false, "Je moet minimaal 18 jaar oud zijn om te registreren.", null);
                }

              // Validate password strength
   var (passwordValid, passwordErrors) = model.ValidatePassword();
     if (!passwordValid)
     {
        return (false, $"Wachtwoord voldoet niet aan de eisen: {string.Join(", ", passwordErrors)}", null);
     }

       // Check if email already exists
            if (await EmailExistsAsync(model.Email))
       {
         return (false, "Dit e-mailadres is al geregistreerd.", null);
      }

           // Get netbeheerder for the user's area
            var netbeheerder = await _netbeheerderService.GetNetbeheerderByPostalCodeAsync(model.PostalCode);

  // Create new user account
             var user = new UserAccount
       {
       FirstName = model.FirstName.Trim(),
      LastName = model.LastName.Trim(),
              DateOfBirth = model.DateOfBirth!.Value,
   PostalCode = model.PostalCode.ToUpper().Replace(" ", ""),
       HouseNumber = model.HouseNumber,
    HouseNumberAddition = model.HouseNumberAddition,
      Street = model.Street,
      City = model.City,
       Email = model.Email.ToLower().Trim(),
       PasswordHash = HashPassword(model.Password),
   HasSmartMeter = model.HasSmartMeter ?? false,
    NetbeheerderId = netbeheerder?.Id,
    NetbeheerderName = netbeheerder?.Name,
       CreatedAt = DateTime.UtcNow,
          IsActive = true,
  EmailVerified = false
   };

        _dbContext.Users.Add(user);
       await _dbContext.SaveChangesAsync();

                // Auto-login after registration
                await SetCurrentUserAsync(user);

       _logger.LogInformation("New user registered: {Email}", user.Email);

     return (true, "Account succesvol aangemaakt!", user);
            }
  catch (Exception ex)
  {
       _logger.LogError(ex, "Error registering user: {Email}", model.Email);
              return (false, "Er is een fout opgetreden bij het registreren. Probeer het later opnieuw.", null);
    }
   }

        public async Task<(bool Success, string Message, UserAccount? User)> LoginAsync(LoginModel model)
        {
    try
       {
         var user = await GetUserByEmailAsync(model.Email);

      if (user == null)
    {
            // Don't reveal that email doesn't exist (security)
         return (false, "Ongeldige e-mail of wachtwoord.", null);
      }

         if (!user.IsActive)
      {
          return (false, "Dit account is gedeactiveerd.", null);
         }

      if (!VerifyPassword(model.Password, user.PasswordHash))
       {
      _logger.LogWarning("Failed login attempt for: {Email}", model.Email);
        return (false, "Ongeldige e-mail of wachtwoord.", null);
      }

      // Update last login timestamp
  await UpdateLastLoginAsync(user.Id);

                // Store user in session
                await SetCurrentUserAsync(user);

  _logger.LogInformation("User logged in: {Email}", user.Email);

         return (true, "Inloggen gelukt!", user);
      }
       catch (Exception ex)
  {
                _logger.LogError(ex, "Error during login for: {Email}", model.Email);
   return (false, "Er is een fout opgetreden bij het inloggen. Probeer het later opnieuw.", null);
            }
        }

        public async Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(USER_SESSION_KEY);
                await Task.CompletedTask;
            }
        }

        public async Task<UserAccount?> GetCurrentUserAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var userJson = session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson)) return null;

            var userInfo = JsonSerializer.Deserialize<UserSessionInfo>(userJson);
            if (userInfo == null) return null;

            // Get fresh user data from database
            return await GetUserByIdAsync(userInfo.UserId);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }

        private async Task SetCurrentUserAsync(UserAccount user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var userInfo = new UserSessionInfo
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}"
                };

                session.SetString(USER_SESSION_KEY, JsonSerializer.Serialize(userInfo));
                await Task.CompletedTask;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
     {
          if (string.IsNullOrWhiteSpace(email))
            return false;

  return await _dbContext.Users
     .AnyAsync(u => u.Email == email.ToLower().Trim());
   }

        public async Task<UserAccount?> GetUserByIdAsync(int userId)
   {
    return await _dbContext.Users
    .FirstOrDefaultAsync(u => u.Id == userId);
   }

        public async Task<UserAccount?> GetUserByEmailAsync(string email)
      {
      if (string.IsNullOrWhiteSpace(email))
   return null;

return await _dbContext.Users
      .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());
  }

  public async Task<bool> UpdateLastLoginAsync(int userId)
        {
       try
   {
       var user = await _dbContext.Users.FindAsync(userId);
    if (user != null)
     {
  user.LastLoginAt = DateTime.UtcNow;
           await _dbContext.SaveChangesAsync();
       return true;
  }
         return false;
            }
            catch (Exception ex)
            {
          _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
 return false;
  }
    }

        /// <summary>
 /// Hash password using BCrypt
  /// </summary>
        public string HashPassword(string password)
 {
            return BCrypt.Net.BCrypt.HashPassword(password, BCRYPT_WORK_FACTOR);
        }

        /// <summary>
        /// Verify password against BCrypt hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
          try
            {
    return BCrypt.Net.BCrypt.Verify(password, hash);
          }
      catch (Exception ex)
            {
 _logger.LogError(ex, "Error verifying password");
     return false;
 }
        }

        // Helper class for session storage
        private class UserSessionInfo
        {
            public int UserId { get; set; }
            public string Email { get; set; } = "";
            public string FullName { get; set; } = "";
        }
    }
}
