using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Service for monitoring gas infrastructure and detecting leaks
 /// Connects to Python API backend
    /// </summary>
    public interface IGasMonitoringService
    {
  /// <summary>
        /// Get current gas usage for a specific location
        /// </summary>
   Task<GasUsage?> GetCurrentUsageAsync(string location);

   /// <summary>
        /// Get all current gas usage across all locations
        /// </summary>
        Task<List<GasUsage>> GetAllCurrentUsageAsync();

        /// <summary>
        /// Get active gas leak alerts
        /// </summary>
 Task<List<GasLeakAlert>> GetActiveAlertsAsync();

 /// <summary>
     /// Get all gas leak alerts (including resolved)
    /// </summary>
        Task<List<GasLeakAlert>> GetAllAlertsAsync();

 /// <summary>
  /// Resolve a gas leak alert
    /// </summary>
        Task<bool> ResolveAlertAsync(string alertId);

      /// <summary>
    /// Get total gas consumption for the city
  /// </summary>
        Task<double> GetTotalConsumptionAsync();
    }
}
