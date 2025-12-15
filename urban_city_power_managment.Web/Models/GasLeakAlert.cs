using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace urban_city_power_managment.Web.Models
{
    /// <summary>
    /// Alert severity levels for gas incidents
    /// </summary>
    public enum AlertSeverity
    {
     Info,
        Low,
      Medium,
        High,
    Critical
    }

    /// <summary>
    /// Represents a gas leak alert or incident
    /// Stored in Azure SQL Database
 /// </summary>
    [Table("GasLeakAlerts")]
    public class GasLeakAlert
{
      [Key]
      public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
 [MaxLength(100)]
 public string Location { get; set; } = string.Empty;

[Column(TypeName = "decimal(18,3)")]
        public decimal PressureDrop { get; set; }

        [Column(TypeName = "decimal(18,3)")]
     public decimal FlowRateAnomaly { get; set; }

        public AlertSeverity Severity { get; set; }

public DateTime DetectedAt { get; set; }

        public bool IsResolved { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

 public int AffectedCustomers { get; set; }
    }
}
