using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace urban_city_power_managment.Web.Models
{
    /// <summary>
    /// Represents real-time gas usage and monitoring data
    /// Stored in Azure SQL Database
    /// </summary>
    [Table("GasUsage")]
    public class GasUsage
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gas flow rate in cubic meters per hour (m³/h)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Gas pressure in bar
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal Pressure { get; set; }

        /// <summary>
        /// Gas temperature in Celsius
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Temperature { get; set; }

        /// <summary>
        /// Total consumption in cubic meters (m³)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal TotalConsumption { get; set; }

        /// <summary>
        /// Location/zone identifier
        /// </summary>
        [Required]
        [MaxLength(100)]
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
