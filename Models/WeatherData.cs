using System;

namespace urban_city_power_managment.Models
{
    /// <summary>
    /// Weather data model for Open-Meteo API response
    /// </summary>
    public class WeatherData
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }
    public int WeatherCode { get; set; }
   public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
        public double Humidity { get; set; }
        public double Precipitation { get; set; }
    public double Pressure { get; set; }
        
   /// <summary>
        /// Get weather description from WMO weather code
        /// https://www.nodc.noaa.gov/archive/arc0021/0002199/1.1/data/0-data/HTML/WMO-CODE/WMO4677.HTM
        /// </summary>
        public static string GetWeatherDescription(int code)
     {
    return code switch
{
                0 => "Clear sky",
  1 => "Mainly clear",
          2 => "Partly cloudy",
        3 => "Overcast",
         45 => "Foggy",
    48 => "Depositing rime fog",
        51 => "Light drizzle",
      53 => "Moderate drizzle",
         55 => "Dense drizzle",
          61 => "Slight rain",
   63 => "Moderate rain",
          65 => "Heavy rain",
       71 => "Slight snow",
      73 => "Moderate snow",
     75 => "Heavy snow",
                77 => "Snow grains",
           80 => "Slight rain showers",
        81 => "Moderate rain showers",
            82 => "Violent rain showers",
      85 => "Slight snow showers",
                86 => "Heavy snow showers",
   95 => "Thunderstorm",
      96 => "Thunderstorm with slight hail",
          99 => "Thunderstorm with heavy hail",
     _ => "Unknown"
    };
    }
    }
}
