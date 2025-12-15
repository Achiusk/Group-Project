using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace urban_city_power_managment.Web.Models
{
    /// <summary>
    /// P1 Sensor data model for Dutch smart energy meters (DSMR/P1)
    /// Used to capture energy consumption data from consumers
    /// Stored in Azure SQL Database
    /// </summary>
    [Table("P1SensorData")]
    public class P1SensorData
    {
        /// <summary>
/// Unique identifier for the measurement
   /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Meter ID / Equipment identifier
        /// </summary>
        [Required]
     [MaxLength(50)]
        public string MeterId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the measurement
 /// </summary>
     [Required]
        public DateTime Timestamp { get; set; }

    /// <summary>
  /// Consumer/household identifier
        /// </summary>
    [Required]
 [MaxLength(50)]
      public string ConsumerId { get; set; } = string.Empty;

        /// <summary>
        /// Consumer address location in Eindhoven
        /// </summary>
        [MaxLength(200)]
 public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Postcode (Dutch format: 1234 AB)
        /// </summary>
        [MaxLength(10)]
        public string Postcode { get; set; } = string.Empty;

    /// <summary>
        /// Neighborhood/Wijk in Eindhoven
        /// </summary>
 [MaxLength(50)]
  public string Wijk { get; set; } = string.Empty;

     // ELECTRICITY DATA
        /// <summary>
      /// Electricity delivered tariff 1 (low/night) in kWh
        /// DSMR: 1-0:1.8.1
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
    public decimal ElectricityDeliveredTariff1 { get; set; }

        /// <summary>
 /// Electricity delivered tariff 2 (high/day) in kWh
    /// DSMR: 1-0:1.8.2
   /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal ElectricityDeliveredTariff2 { get; set; }

        /// <summary>
        /// Electricity returned tariff 1 (solar return low) in kWh
     /// DSMR: 1-0:2.8.1
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal ElectricityReturnedTariff1 { get; set; }

        /// <summary>
        /// Electricity returned tariff 2 (solar return high) in kWh
    /// DSMR: 1-0:2.8.2
   /// </summary>
     [Column(TypeName = "decimal(18,3)")]
        public decimal ElectricityReturnedTariff2 { get; set; }

        /// <summary>
        /// Current electricity tariff indicator (1 or 2)
/// DSMR: 0-0:96.14.0
   /// </summary>
   public int CurrentTariff { get; set; }

     /// <summary>
        /// Current power consumption in kW
        /// DSMR: 1-0:1.7.0
      /// </summary>
      [Column(TypeName = "decimal(18,3)")]
        public decimal CurrentPowerConsumption { get; set; }

        /// <summary>
        /// Current power return (solar) in kW
        /// DSMR: 1-0:2.7.0
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal CurrentPowerReturn { get; set; }

        // GAS DATA
        /// <summary>
        /// Total gas consumption in m³
        /// DSMR: 0-1:24.2.1
        /// </summary>
      [Column(TypeName = "decimal(18,3)")]
        public decimal GasConsumption { get; set; }

    /// <summary>
        /// Gas consumption timestamp
        /// </summary>
        public DateTime? GasTimestamp { get; set; }

        // CALCULATED PROPERTIES (not stored in DB)
   /// <summary>
    /// Total electricity delivered (both tariffs) in kWh
  /// </summary>
        [NotMapped]
      public decimal TotalElectricityDelivered =>
         ElectricityDeliveredTariff1 + ElectricityDeliveredTariff2;

        /// <summary>
        /// Total electricity returned (both tariffs) in kWh
  /// </summary>
        [NotMapped]
        public decimal TotalElectricityReturned =>
            ElectricityReturnedTariff1 + ElectricityReturnedTariff2;

   /// <summary>
        /// Net electricity consumption in kWh
        /// </summary>
        [NotMapped]
        public decimal NetElectricityConsumption =>
            TotalElectricityDelivered - TotalElectricityReturned;

    /// <summary>
        /// Current net power (positive = consuming, negative = returning)
        /// </summary>
        [NotMapped]
        public decimal CurrentNetPower =>
            CurrentPowerConsumption - CurrentPowerReturn;

        /// <summary>
        /// Is currently returning power to grid (solar active)
        /// </summary>
      [NotMapped]
        public bool IsReturningPower => CurrentPowerReturn > CurrentPowerConsumption;

        // Navigation property
        public virtual Consumer? Consumer { get; set; }
    }

    /// <summary>
    /// Consumer (household) entity
    /// </summary>
    [Table("Consumers")]
    public class Consumer
    {
        [Key]
        [MaxLength(50)]
      public string ConsumerId { get; set; } = string.Empty;

        [Required]
   [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
     public string Address { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Wijk { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Postcode { get; set; } = string.Empty;

  public bool HasSolarPanels { get; set; }

   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 public bool IsActive { get; set; } = true;

        // Navigation property
     public virtual ICollection<P1SensorData> SensorReadings { get; set; } = new List<P1SensorData>();
    }

    /// <summary>
    /// Aggregated consumer energy data for dashboard display (DTO)
    /// </summary>
    public class ConsumerEnergyData
    {
   public string ConsumerId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
     public string Wijk { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;

     /// <summary>
        /// Current power consumption in kW
        /// </summary>
  public double CurrentPowerKw { get; set; }

        /// <summary>
        /// Current solar return in kW
        /// </summary>
        public double CurrentSolarReturnKw { get; set; }

        /// <summary>
      /// Today's total consumption in kWh
        /// </summary>
        public double TodayConsumptionKwh { get; set; }

    /// <summary>
        /// Today's solar return in kWh
        /// </summary>
        public double TodaySolarReturnKwh { get; set; }

    /// <summary>
        /// This month's gas consumption in m³
        /// </summary>
     public double MonthGasConsumptionM3 { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
   public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Has solar panels installed
   /// </summary>
        public bool HasSolarPanels { get; set; }
    }

    /// <summary>
    /// Aggregated statistics for a neighborhood (Wijk) - DTO
    /// </summary>
    public class WijkEnergyStatistics
    {
   public string WijkName { get; set; } = string.Empty;
  public int TotalHouseholds { get; set; }
        public int HouseholdsWithSolar { get; set; }
   public double TotalCurrentConsumptionKw { get; set; }
        public double TotalCurrentSolarReturnKw { get; set; }
    public double TodayTotalConsumptionKwh { get; set; }
        public double TodayTotalSolarReturnKwh { get; set; }
  public double MonthTotalGasM3 { get; set; }
 public double AverageConsumptionPerHousehold { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
