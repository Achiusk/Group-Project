using System.Threading.Tasks;

namespace urban_city_power_managment.Services
{
    public interface IAuthenticationService
    {
     Task<AuthenticationResult> SignInAsync();
  Task SignOutAsync();
   Task<bool> IsAuthenticatedAsync();
     Task<string?> GetCurrentUserAsync();
    }

    public class AuthenticationResult
    {
 public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
   public string? AccessToken { get; set; }
 public string? UserName { get; set; }
    }
}
