using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;
using System.Text.Json;
using System.Security.Cryptography;

namespace urban_city_power_managment.Web.Services
{
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
        private readonly CustomAuthStateProvider _authStateProvider;

        private const int BCRYPT_WORK_FACTOR = 12;
        private const string USER_SESSION_KEY = "SecureAuth_User_v1";
        private const string SESSION_TOKEN_KEY = "SecureAuth_Token_v1";

        public AuthService(
            EnergyDbContext dbContext,
            ILogger<AuthService> logger,
            INetbeheerderService netbeheerderService,
            IHttpContextAccessor httpContextAccessor,
            CustomAuthStateProvider authStateProvider)
        {
            _dbContext = dbContext;
            _logger = logger;
            _netbeheerderService = netbeheerderService;
            _httpContextAccessor = httpContextAccessor;
            _authStateProvider = authStateProvider;
        }

        public async Task<(bool Success, string Message, UserAccount? User)> RegisterAsync(RegistrationModel model)
        {
            try
            {
                if (!model.IsAtLeast18YearsOld())
                {
                    return (false, "Je moet minimaal 18 jaar oud zijn om te registreren.", null);
                }

                var (passwordValid, passwordErrors) = ValidatePasswordStrength(model.Password);
                if (!passwordValid)
                {
                    return (false, $"Wachtwoord voldoet niet aan de eisen: {string.Join(", ", passwordErrors)}", null);
                }

                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "Dit e-mailadres is al geregistreerd.", null);
                }

                var netbeheerder = await _netbeheerderService.GetNetbeheerderByPostalCodeAsync(model.PostalCode);

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

                // Set session and notify auth state provider
                await SetCurrentUserAsync(user);
                _authStateProvider.MarkUserAsAuthenticated(user);

                _logger.LogInformation("New user registered: {Email}", MaskEmail(user.Email));

                return (true, "Account succesvol aangemaakt!", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
                return (false, "Er is een fout opgetreden bij het registreren. Probeer het later opnieuw.", null);
            }
        }

        public async Task<(bool Success, string Message, UserAccount? User)> LoginAsync(LoginModel model)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", MaskEmail(model.Email));
                
                // Add small delay to prevent timing attacks
                await Task.Delay(RandomNumberGenerator.GetInt32(50, 150));

                var user = await GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email {Email}", MaskEmail(model.Email));
                    return (false, "Ongeldige e-mail of wachtwoord.", null);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: Account deactivated for user {UserId}", user.Id);
                    return (false, "Dit account is gedeactiveerd.", null);
                }

                if (!VerifyPassword(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                    return (false, "Ongeldige e-mail of wachtwoord.", null);
                }

                await UpdateLastLoginAsync(user.Id);
                await SetCurrentUserAsync(user);
                
                // Notify auth state provider - this is the key fix!
                _authStateProvider.MarkUserAsAuthenticated(user);

                _logger.LogInformation("Login successful for user {UserId}", user.Id);

                return (true, "Inloggen gelukt!", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error: {Message}", ex.Message);
                
                if (ex.Message.Contains("connect") || ex.Message.Contains("database") || ex.Message.Contains("MySQL"))
                {
                    return (false, "Database verbinding niet beschikbaar. Probeer het later opnieuw.", null);
                }
                
                return (false, "Er is een fout opgetreden bij het inloggen. Probeer het later opnieuw.", null);
            }
        }

        public async Task LogoutAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(USER_SESSION_KEY);
                session.Remove(SESSION_TOKEN_KEY);
                session.Clear();
            }
            
            // Notify auth state provider
            _authStateProvider.MarkUserAsLoggedOut();
            
            await Task.CompletedTask;
        }

        public async Task<UserAccount?> GetCurrentUserAsync()
        {
            // First check if auth state provider has user
            var userId = _authStateProvider.GetCurrentUserId();
            if (userId.HasValue)
            {
                return await GetUserByIdAsync(userId.Value);
            }

            // Fallback to session check
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var sessionToken = session.GetString(SESSION_TOKEN_KEY);
            if (string.IsNullOrEmpty(sessionToken)) return null;

            var userJson = session.GetString(USER_SESSION_KEY);
            if (string.IsNullOrEmpty(userJson)) return null;

            try
            {
                var userInfo = JsonSerializer.Deserialize<SecureUserSessionInfo>(userJson);
                if (userInfo == null) return null;

                if (userInfo.SessionToken != sessionToken) return null;

                var user = await GetUserByIdAsync(userInfo.UserId);
                
                // Restore auth state if session is valid but auth state was lost
                if (user != null && !_authStateProvider.IsAuthenticated)
                {
                    _authStateProvider.MarkUserAsAuthenticated(user);
                }
                
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            // First check auth state provider (faster, no DB call)
            if (_authStateProvider.IsAuthenticated)
            {
                return true;
            }
            
            // Fallback to full check
            var user = await GetCurrentUserAsync();
            return user != null;
        }

        private async Task SetCurrentUserAsync(UserAccount user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var sessionToken = GenerateSecureToken();

                var userInfo = new SecureUserSessionInfo
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}",
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow.Ticks
                };

                session.SetString(SESSION_TOKEN_KEY, sessionToken);
                session.SetString(USER_SESSION_KEY, JsonSerializer.Serialize(userInfo));
                await Task.CompletedTask;
            }
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

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

            var weakPasswords = new[] { "password", "123456", "qwerty", "welkom", "wachtwoord" };
            if (weakPasswords.Any(w => password.ToLower().Contains(w)))
                errors.Add("Wachtwoord is te eenvoudig");

            return (errors.Count == 0, errors);
        }

        private static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Trim()
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");
        }

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

            try
            {
                return await _dbContext.Users
                    .AnyAsync(u => u.Email == email.ToLower().Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence");
                return false;
            }
        }

        public async Task<UserAccount?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by id: {UserId}", userId);
                return null;
            }
        }

        public async Task<UserAccount?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                return await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email");
                return null;
            }
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
                _logger.LogError(ex, "Error updating last login");
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCRYPT_WORK_FACTOR);
        }

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
