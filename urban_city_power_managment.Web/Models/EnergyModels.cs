using System;

namespace urban_city_power_managment.Web.Models
{
/// <summary>
    /// Power generation data for the city
  /// </summary>
    public class PowerGeneration
    {
    /// <summary>
        /// Wind power generation in MW
        /// </summary>
 public double WindPowerMw { get; set; }

/// <summary>
        /// Wind power percentage of total
   /// </summary>
        public double WindPercentage { get; set; }

        /// <summary>
        /// Water/hydro power generation in MW
        /// </summary>
    public double WaterPowerMw { get; set; }

        /// <summary>
/// Water power percentage of total
        /// </summary>
        public double WaterPercentage { get; set; }

   /// <summary>
        /// Coal power generation in MW
        /// </summary>
        public double CoalPowerMw { get; set; }

        /// <summary>
     /// Coal power percentage of total
        /// </summary>
        public double CoalPercentage { get; set; }

        /// <summary>
        /// Solar power generation in MW (from city solar installations)
   /// </summary>
    public double SolarPowerMw { get; set; }

     /// <summary>
        /// Solar power percentage of total
     /// </summary>
        public double SolarPercentage { get; set; }

        /// <summary>
/// Total power generation in MW
        /// </summary>
        public double TotalGenerationMw { get; set; }

        /// <summary>
   /// Total renewable percentage
    /// </summary>
        public double RenewablePercentage => WindPercentage + WaterPercentage + SolarPercentage;

        /// <summary>
  /// Timestamp of the data
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Business/commercial energy consumer
    /// </summary>
  public class BusinessConsumer
    {
     public string Id { get; set; } = Guid.NewGuid().ToString();
      public string Name { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
        public string Wijk { get; set; } = string.Empty;
        
        /// <summary>
  /// Current power consumption in MW
     /// </summary>
        public double CurrentConsumptionMw { get; set; }
        
        /// <summary>
        /// Monthly consumption in MWh
 /// </summary>
    public double MonthlyConsumptionMwh { get; set; }
      
        /// <summary>
   /// Has own renewable energy source
      /// </summary>
    public bool HasRenewableSource { get; set; }
        
   /// <summary>
        /// Renewable energy percentage if applicable
   /// </summary>
        public double RenewablePercentage { get; set; }
    }

    /// <summary>
    /// Notification/alert for the system
    /// </summary>
  public class SystemNotification
    {
   public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
 public string Message { get; set; } = string.Empty;
     public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
     public string? RelatedEntityId { get; set; }
    }

  public enum NotificationType
    {
        GasLeak,
     HighUsage,
        LowPressure,
        SystemAlert,
        Maintenance,
  WeatherWarning
    }

    public enum NotificationSeverity
    {
 Info,
 Warning,
 Error,
    Critical
    }

    /// <summary>
    /// Task for maintenance/operations
    /// </summary>
    public class MaintenanceTask
  {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
     public DateTime? CompletedAt { get; set; }
 public string AssignedTo { get; set; } = string.Empty;
    }

   public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
      Completed,
    Cancelled
    }
}
