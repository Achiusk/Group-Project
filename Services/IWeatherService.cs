using System.Threading.Tasks;
using urban_city_power_managment.Models;

namespace urban_city_power_managment.Services
{
  public interface IWeatherService
  {
     /// <summary>
        /// Get current weather for Eindhoven
 /// </summary>
        Task<WeatherData> GetCurrentWeatherAsync();
      
    /// <summary>
        /// Get weather for specific coordinates
  /// </summary>
        Task<WeatherData> GetWeatherForLocationAsync(double latitude, double longitude);
    }
}
