using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using urban_city_power_managment.Models;

namespace urban_city_power_managment.Services
{
    /// <summary>
 /// Weather service using Open-Meteo KNMI API (Royal Netherlands Meteorological Institute)
    /// Free API - No authentication required
    /// Documentation: https://open-meteo.com/en/docs/knmi-api
    /// </summary>
 public class OpenMeteoWeatherService : IWeatherService
    {
   private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "https://api.open-meteo.com/v1/knmi";
        
  // Eindhoven coordinates
        private const double EINDHOVEN_LATITUDE = 51.4416;
        private const double EINDHOVEN_LONGITUDE = 5.4697;

  public OpenMeteoWeatherService()
{
      _httpClient = new HttpClient();
   _httpClient.DefaultRequestHeaders.Add("User-Agent", "UrbanEnergyManagement/1.0");
   }

        public async Task<WeatherData> GetCurrentWeatherAsync()
    {
   return await GetWeatherForLocationAsync(EINDHOVEN_LATITUDE, EINDHOVEN_LONGITUDE);
        }

   public async Task<WeatherData> GetWeatherForLocationAsync(double latitude, double longitude)
        {
   try
       {
      // Build API URL with parameters - KNMI API correct endpoint
     var url = $"{API_BASE_URL}?latitude={latitude:F4}&longitude={longitude:F4}" +
    "&current=temperature_2m,relative_humidity_2m,precipitation,weather_code,wind_speed_10m,wind_direction_10m,pressure_msl" +
       "&timezone=Europe%2FAmsterdam";

  var response = await _httpClient.GetAsync(url);
   response.EnsureSuccessStatusCode();

       var jsonString = await response.Content.ReadAsStringAsync();
var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;

  // Parse current weather data
     var current = root.GetProperty("current");
        
 var weatherData = new WeatherData
    {
      Temperature = current.GetProperty("temperature_2m").GetDouble(),
         Humidity = current.GetProperty("relative_humidity_2m").GetDouble(),
   Precipitation = current.GetProperty("precipitation").GetDouble(),
          WeatherCode = current.GetProperty("weather_code").GetInt32(),
       WindSpeed = current.GetProperty("wind_speed_10m").GetDouble(),
      WindDirection = current.GetProperty("wind_direction_10m").GetDouble(),
     Pressure = current.GetProperty("pressure_msl").GetDouble(),
     Timestamp = DateTime.Parse(current.GetProperty("time").GetString()!)
     };

       weatherData.Description = WeatherData.GetWeatherDescription(weatherData.WeatherCode);

   return weatherData;
      }
   catch (Exception ex)
      {
      // Log error and return default data
      Console.WriteLine($"Error fetching weather data: {ex.Message}");
       
     // Return fallback data - Late November/Winter typical for Eindhoven
      return new WeatherData
   {
    Temperature = 8.0,  // More realistic for late November in NL
  Humidity = 85,
     Precipitation = 0.2,
      WeatherCode = 3,  // Overcast
     Description = "Overcast (offline)",
   WindSpeed = 15,
    WindDirection = 225,  // Southwest - typical for NL
  Pressure = 1010,
           Timestamp = DateTime.Now
    };
       }
        }

  /// <summary>
        /// Get formatted weather string for display
        /// </summary>
        public string GetFormattedWeatherString(WeatherData weather)
        {
  return $"{weather.Description}, {weather.Temperature:F1}°C";
   }

  /// <summary>
        /// Check if weather conditions are favorable for renewable energy
        /// </summary>
        public bool IsFavorableForRenewableEnergy(WeatherData weather)
   {
       // Good wind speed for turbines: 4-25 m/s
   // Good solar conditions: clear to partly cloudy
      bool goodWind = weather.WindSpeed >= 4 && weather.WindSpeed <= 25;
       bool goodSolar = weather.WeatherCode <= 2; // Clear to partly cloudy
          
     return goodWind || goodSolar;
  }
    }
}
