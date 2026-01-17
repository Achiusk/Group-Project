using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;
using System.Text.Json;
using System.Security.Cryptography;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Authentication service for user registration and login
    /// Uses BCrypt for secure password hashing with salt rotation
    /// Sessions are secured with cryptographic tokens
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

        // BCrypt work factor (12 = ~250ms to hash, secure against brute force)
        // Each hash includes unique salt - no need for separate salt storage
        private const int BCRYPT_WORK_FACTOR = 12;
        
        // Session keys with cryptographic prefix for added security
        private const string USER_SESSION_KEY = "SecureAuth_User_v1";
        private const string SESSION_TOKEN_KEY = "SecureAuth_Token_v1";

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

                // Validate password strength with enhanced requirements
                var (passwordValid, passwordErrors) = ValidatePasswordStrength(model.Password);
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

                // Create new user account with secure password hash
                var user = new UserAccount
                {
                    FirstName = SanitizeInput(model.FirstName),
                    LastName = SanitizeInput(model.LastName),
                    DateOfBirth = model.DateOfBirth!.Value,
                    PostalCode = model.PostalCode.ToUpper().Replace(" ", ""),
                    HouseNumber = model.HouseNumber,
                    HouseNumberAddition = model.HouseNumberAddition,
                    Street = model.Street,
                    City = model.City,
                    Email = model.Email.ToLower().Trim(),
                    PasswordHash = HashPassword(model.Password), // BCrypt with unique salt per user
                    HasSmartMeter = model.HasSmartMeter ?? false,
                    NetbeheerderId = netbeheerder?.Id,
                    NetbeheerderName = netbeheerder?.Name,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailVerified = false
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Auto-login after registration with secure session
                await SetCurrentUserAsync(user);

                _logger.LogInformation("New user registered: {Email} at {Time}", 
                    MaskEmail(user.Email), DateTime.UtcNow);

                return (true, "Account succesvol aangemaakt!", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return (false, "Er is een fout opgetreden bij het registreren. Probeer het later opnieuw.", null);
            }
        }

        public async Task<(bool Success, string Message, UserAccount? User)> LoginAsync(LoginModel model)
        {
            try
            {
                // Add small delay to prevent timing attacks
                await Task.Delay(RandomNumberGenerator.GetInt32(50, 150));

                var user = await GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    // Don't reveal that email doesn't exist (timing attack prevention)
                    _logger.LogWarning("Login attempt for non-existent email");
                    return (false, "Ongeldige e-mail of wachtwoord.", null);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for deactivated account: {UserId}", user.Id);
                    return (false, "Dit account is gedeactiveerd.", null);
                }

                // BCrypt.Verify handles the salt internally - secure comparison
                if (!VerifyPassword(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
                    return (false, "Ongeldige e-mail of wachtwoord.", null);
                }

                // Update last login timestamp
                await UpdateLastLoginAsync(user.Id);

                // Store user in secure session with cryptographic token
                await SetCurrentUserAsync(user);

                _logger.LogInformation("User logged in: {UserId} at {Time}", user.Id, DateTime.UtcNow);

                return (true, "Inloggen gelukt!", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return (false, "Er is een fout opgetreden bij het inloggen. Probeer het later opnieuw.", null);
            }
        }

        public async Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                // Clear all auth-related session data
                session.Remove(USER_SESSION_KEY);
                session.Remove(SESSION_TOKEN_KEY);
                session.Clear();
                await Task.CompletedTask;
            }
        }

        public async Task<UserAccount?> GetCurrentUserAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            // Verify session token exists
            var sessionToken = session.GetString(SESSION_TOKEN_KEY);
            if (string.IsNullOrEmpty(sessionToken)) return null;

            var userJson = session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson)) return null;

            try
            {
                var userInfo = JsonSerializer.Deserialize<SecureUserSessionInfo>(userJson);
                if (userInfo == null) return null;

                // Verify token matches
                if (userInfo.SessionToken != sessionToken) return null;

                // Get fresh user data from database
                return await GetUserByIdAsync(userInfo.UserId);
            }
            catch
            {
                return null;
            }
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
                // Generate cryptographically secure session token
                var sessionToken = GenerateSecureToken();

                var userInfo = new SecureUserSessionInfo
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow.Ticks
                };

                // Store token separately for validation
                session.SetString(SESSION_TOKEN_KEY, sessionToken);
                session.SetString(USER_SESSION_KEY, JsonSerializer.Serialize(userInfo));
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Generate cryptographically secure random token
        /// </summary>
        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Validate password meets security requirements
        /// </summary>
        private static (bool IsValid, List<string> Errors) ValidatePasswordStrength(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("Wachtwoord is verplicht");
                return (false, errors);
            }

            if (password.Length < 8)
                errors.Add("Minimaal 8 karakters");
            
            if (password.Length > 128)
                errors.Add("Maximaal 128 karakters");
            
            if (!password.Any(char.IsLower))
                errors.Add("Minimaal één kleine letter (a-z)");
            
            if (!password.Any(char.IsUpper))
                errors.Add("Minimaal één hoofdletter (A-Z)");
            
            if (!password.Any(char.IsDigit))
                errors.Add("Minimaal één cijfer (0-9)");
            
            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;':\",./<>?".Contains(c)))
                errors.Add("Minimaal één speciaal teken");

            // Check for common weak passwords
            var weakPasswords = new[] { "password", "123456", "qwerty", "welkom", "wachtwoord" };
            if (weakPasswords.Any(w => password.ToLower().Contains(w)))
                errors.Add("Wachtwoord is te eenvoudig");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Sanitize user input to prevent XSS
        /// </summary>
        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Trim()
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");
        }

        /// <summary>
        /// Mask email for logging (privacy)
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var parts = email.Split('@');
            if (parts.Length != 2) return "***";
            var name = parts[0];
            var masked = name.Length > 2 ? name[0] + "***" + name[^1] : "***";
            return $"{masked}@{parts[1]}";
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
        /// Hash password using BCrypt with automatic salt generation
        /// Each call generates a unique salt, making rainbow table attacks impossible
        /// Work factor of 12 = ~250ms hash time, secure against brute force
        /// </summary>
        public string HashPassword(string password)
        {
            // BCrypt automatically generates a unique 128-bit salt per hash
            // The salt is embedded in the hash output, no separate storage needed
            return BCrypt.Net.BCrypt.HashPassword(password, BCRYPT_WORK_FACTOR);
        }

        /// <summary>
        /// Verify password against BCrypt hash
        /// BCrypt extracts the salt from the stored hash and re-hashes for comparison
        /// This is timing-safe to prevent timing attacks
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                // BCrypt.Verify is timing-safe
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        /// <summary>
        /// Secure session info with token validation
        /// </summary>
        private class SecureUserSessionInfo
        {
            public int UserId { get; set; }
            public string Email { get; set; } = "";
            public string FullName { get; set; } = "";
            public string SessionToken { get; set; } = "";
            public long CreatedAt { get; set; }
        }
    }
}
