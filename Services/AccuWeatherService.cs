using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using urban_city_power_managment.Models;

namespace urban_city_power_managment.Services
{
    /// <summary>
    /// Legacy AccuWeather service - kept for reference
    /// Use OpenMeteoWeatherService for production
    /// </summary>
    public class AccuWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AccuWeatherService(string apiKey = "")
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
        }

        public async Task<WeatherData> GetCurrentWeatherAsync()
        {
            // Fallback implementation
            return await GetWeatherForLocationAsync(51.4416, 5.4697);
        }

        public async Task<WeatherData> GetWeatherForLocationAsync(double latitude, double longitude)
        {
            // Return mock data - AccuWeather requires API key
            return new WeatherData
            {
                Temperature = 15.0,
                Humidity = 70,
                Precipitation = 0,
                WeatherCode = 1,
                Description = "Partly cloudy (Mock)",
                WindSpeed = 10,
                WindDirection = 180,
                Pressure = 1013,
                Timestamp = DateTime.Now
            };
        }
    }
}
