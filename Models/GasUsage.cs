using System;

namespace urban_city_power_managment.Models
{
    /// <summary>
    /// Represents real-time gas usage and monitoring data
    /// </summary>
    public class GasUsage
    {
        /// <summary>
        /// Gas flow rate in cubic meters per hour (m³/h)
     /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gas pressure in bar
        /// </summary>
        public double Pressure { get; set; }

    /// <summary>
        /// Gas temperature in Celsius
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Total consumption in cubic meters (m³)
        /// </summary>
        public double TotalConsumption { get; set; }

        /// <summary>
        /// Location/zone identifier
        /// </summary>
        public string Location { get; set; } = string.Empty;

/// <summary>
        /// Timestamp of the measurement
        /// </summary>
 public DateTime Timestamp { get; set; }

        /// <summary>
   /// Indicates if the readings are within normal parameters
      /// </summary>
    public bool IsNormal { get; set; }
    }
}
