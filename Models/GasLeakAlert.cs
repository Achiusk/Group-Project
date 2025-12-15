using System;

namespace urban_city_power_managment.Models
{
    /// <summary>
    /// Alert severity levels for gas incidents
    /// </summary>
    public enum AlertSeverity
    {
      /// <summary>
      /// Informational alert - no action required
        /// </summary>
        Info,

    /// <summary>
   /// Low severity - minor concern, monitor situation
     /// </summary>
      Low,

        /// <summary>
   /// Medium severity - investigation required
     /// </summary>
        Medium,

        /// <summary>
        /// High severity - immediate attention required
        /// </summary>
        High,

 /// <summary>
   /// Critical - emergency response required
        /// </summary>
      Critical
    }

    /// <summary>
  /// Represents a gas leak alert or incident
    /// </summary>
    public class GasLeakAlert
    {
        /// <summary>
        /// Unique identifier for the alert
  /// </summary>
 public string Id { get; set; } = Guid.NewGuid().ToString();

      /// <summary>
        /// Location of the detected leak
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
    /// Pressure drop in bar (indicates leak severity)
   /// </summary>
        public double PressureDrop { get; set; }

        /// <summary>
     /// Current flow rate anomaly in m³/h
        /// </summary>
        public double FlowRateAnomaly { get; set; }

        /// <summary>
        /// Alert severity level
    /// </summary>
        public AlertSeverity Severity { get; set; }

/// <summary>
      /// When the leak was detected
        /// </summary>
     public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Indicates if the leak has been resolved
        /// </summary>
     public bool IsResolved { get; set; }

        /// <summary>
  /// When the leak was resolved (if applicable)
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Description of the incident
        /// </summary>
        public string Description { get; set; } = string.Empty;

   /// <summary>
        /// Estimated number of affected customers
      /// </summary>
        public int AffectedCustomers { get; set; }
    }
}
