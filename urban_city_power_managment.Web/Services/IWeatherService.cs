using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Weather service interface
    /// </summary>
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
