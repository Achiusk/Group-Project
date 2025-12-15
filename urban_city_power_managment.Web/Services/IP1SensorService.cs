using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Service interface for P1 sensor data from SQL Database
    /// Handles Dutch smart meter (DSMR) data for consumer energy monitoring
    /// </summary>
    public interface IP1SensorService
    {
        /// <summary>
        /// Get latest P1 data for all consumers
        /// </summary>
        Task<List<P1SensorData>> GetLatestReadingsAsync();

        /// <summary>
        /// Get latest P1 data for a specific consumer
        /// </summary>
        Task<P1SensorData?> GetLatestConsumerReadingAsync(string consumerId);

        /// <summary>
        /// Get aggregated consumer energy data for dashboard
        /// </summary>
        Task<List<ConsumerEnergyData>> GetConsumerSummariesAsync();

        /// <summary>
        /// Get statistics per neighborhood (Wijk) in Eindhoven
        /// </summary>
        Task<List<WijkEnergyStatistics>> GetWijkStatisticsAsync();

        /// <summary>
        /// Get historical data for a consumer
        /// </summary>
        Task<List<P1SensorData>> GetConsumerHistoryAsync(string consumerId, DateTime from, DateTime to);

        /// <summary>
        /// Get total city-wide energy statistics
        /// </summary>
        Task<CityEnergyStatistics> GetCityStatisticsAsync();

        /// <summary>
        /// Save new P1 sensor reading to database
        /// </summary>
        Task<bool> SaveReadingAsync(P1SensorData reading);

        /// <summary>
        /// Get all consumers
        /// </summary>
        Task<List<Consumer>> GetAllConsumersAsync();

        /// <summary>
        /// Get consumers by wijk
        /// </summary>
        Task<List<Consumer>> GetConsumersByWijkAsync(string wijk);
    }

    /// <summary>
    /// City-wide energy statistics
    /// </summary>
    public class CityEnergyStatistics
    {
        public int TotalHouseholds { get; set; }
        public int HouseholdsWithSolar { get; set; }
        public double TotalCurrentConsumptionMw { get; set; }
        public double TotalCurrentSolarReturnMw { get; set; }
        public double TodayTotalConsumptionMwh { get; set; }
        public double TodayTotalSolarReturnMwh { get; set; }
        public double MonthTotalGasM3 { get; set; }
        public double AverageConsumptionPerHousehold { get; set; }
        public double SolarAdoptionPercentage => TotalHouseholds > 0
            ? (double)HouseholdsWithSolar / TotalHouseholds * 100 : 0;
        public DateTime LastUpdate { get; set; }
    }
}
