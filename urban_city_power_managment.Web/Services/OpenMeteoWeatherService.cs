using System.Net.Http.Json;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
   /// <summary>
    /// Weather service using Open-Meteo API (free, no API key required)
    /// </summary>
    public class OpenMeteoWeatherService : IWeatherService
    {
    private readonly HttpClient _httpClient;
     private readonly ILogger<OpenMeteoWeatherService> _logger;

     // Eindhoven coordinates
      private const double EindhovenLatitude = 51.4416;
   private const double EindhovenLongitude = 5.4697;

    public OpenMeteoWeatherService(HttpClient httpClient, ILogger<OpenMeteoWeatherService> logger)
        {
    _httpClient = httpClient;
     _logger = logger;
 _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
   }

 public async Task<WeatherData> GetCurrentWeatherAsync()
    {
    return await GetWeatherForLocationAsync(EindhovenLatitude, EindhovenLongitude);
        }

        public async Task<WeatherData> GetWeatherForLocationAsync(double latitude, double longitude)
        {
    try
   {
          var url = $"forecast?latitude={latitude}&longitude={longitude}" +
"&current=temperature_2m,relative_humidity_2m,precipitation,pressure_msl,wind_speed_10m,wind_direction_10m,weather_code" +
        "&timezone=Europe%2FAmsterdam";

   var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);

 if (response?.Current != null)
      {
 return new WeatherData
    {
            Temperature = response.Current.Temperature2m,
     Humidity = response.Current.RelativeHumidity2m,
            Precipitation = response.Current.Precipitation,
    Pressure = response.Current.PressureMsl,
 WindSpeed = response.Current.WindSpeed10m,
       WindDirection = response.Current.WindDirection10m,
            WeatherCode = response.Current.WeatherCode,
 Description = WeatherData.GetWeatherDescriptionDutch(response.Current.WeatherCode),
  Timestamp = DateTime.Now
      };
         }
       }
          catch (Exception ex)
 {
            _logger.LogWarning(ex, "Failed to fetch weather data, using fallback");
          }

            // Return fallback data
   return new WeatherData
            {
        Temperature = 15.5,
 Humidity = 72,
      Precipitation = 0,
        Pressure = 1013,
        WindSpeed = 12,
   WindDirection = 225,
               WeatherCode = 2,
  Description = WeatherData.GetWeatherDescriptionDutch(2),
      Timestamp = DateTime.Now
 };
        }

  #region Open-Meteo Response Models

        private class OpenMeteoResponse
  {
          public CurrentWeather? Current { get; set; }
 }

 private class CurrentWeather
        {
 public double Temperature2m { get; set; }
      public double RelativeHumidity2m { get; set; }
 public double Precipitation { get; set; }
     public double PressureMsl { get; set; }
        public double WindSpeed10m { get; set; }
      public double WindDirection10m { get; set; }
         public int WeatherCode { get; set; }
  }

        #endregion
    }
}
