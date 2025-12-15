using System;

namespace urban_city_power_managment.Web.Models
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
        /// Get weather description from WMO weather code (Dutch)
        /// </summary>
     public static string GetWeatherDescriptionDutch(int code)
        {
  return code switch
{
              0 => "Onbewolkt",
                1 => "Overwegend helder",
        2 => "Half bewolkt",
       3 => "Bewolkt",
   45 => "Mistig",
    48 => "Rijp",
         51 => "Lichte motregen",
                53 => "Matige motregen",
  55 => "Dichte motregen",
     61 => "Lichte regen",
                63 => "Matige regen",
                65 => "Zware regen",
      71 => "Lichte sneeuw",
        73 => "Matige sneeuw",
   75 => "Zware sneeuw",
      77 => "Sneeuwkorrels",
     80 => "Lichte regenbuien",
                81 => "Matige regenbuien",
             82 => "Hevige regenbuien",
85 => "Lichte sneeuwbuien",
        86 => "Zware sneeuwbuien",
 95 => "Onweer",
    96 => "Onweer met lichte hagel",
                99 => "Onweer met zware hagel",
        _ => "Onbekend"
            };
        }

        /// <summary>
        /// Get weather description from WMO weather code (English)
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
