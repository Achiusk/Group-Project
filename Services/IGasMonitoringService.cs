using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using urban_city_power_managment.Models;

namespace urban_city_power_managment.Services
{
    /// <summary>
    /// Service for monitoring gas infrastructure and detecting leaks
    /// </summary>
    public interface IGasMonitoringService
    {
        /// <summary>
        /// Get current gas usage for a specific location
    /// </summary>
        Task<GasUsage> GetCurrentUsageAsync(string location);

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
        /// Check for potential gas leaks based on pressure and flow data
        /// </summary>
     Task<bool> CheckForLeaksAsync();

      /// <summary>
        /// Resolve a gas leak alert
        /// </summary>
     Task<bool> ResolveAlertAsync(string alertId);

        /// <summary>
        /// Get total gas consumption for the city
    /// </summary>
        Task<double> GetTotalConsumptionAsync();

        /// <summary>
        /// Event raised when a new gas leak is detected
        /// </summary>
    event EventHandler<GasLeakAlert>? LeakDetected;

     /// <summary>
        /// Event raised when gas usage changes significantly
    /// </summary>
        event EventHandler<GasUsage>? UsageChanged;
    }
}
