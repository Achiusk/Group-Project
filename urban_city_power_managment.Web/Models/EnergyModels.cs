using System;

namespace urban_city_power_managment.Web.Models
{
    /// <summary>
    /// Power generation data for the city (used for weather impact display)
    /// </summary>
    public class PowerGeneration
    {
        public double WindPowerMw { get; set; }
        public double WindPercentage { get; set; }
        public double WaterPowerMw { get; set; }
        public double WaterPercentage { get; set; }
        public double CoalPowerMw { get; set; }
        public double CoalPercentage { get; set; }
        public double SolarPowerMw { get; set; }
        public double SolarPercentage { get; set; }
        public double TotalGenerationMw { get; set; }
        public double RenewablePercentage => WindPercentage + WaterPercentage + SolarPercentage;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// City-wide power statistics
    /// </summary>
    public class CityPowerStatistics
    {
        public double TotalDemandMw { get; set; }
        public double TotalSupplyMw { get; set; }
        public double GridBalanceMw => TotalSupplyMw - TotalDemandMw;
        public bool IsDeficit => GridBalanceMw < 0;
        public double RenewablePercentage { get; set; }
        public double CO2SavedTonnes { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
