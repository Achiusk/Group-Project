using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using urban_city_power_managment.Web.Data;
using urban_city_power_managment.Web.Models;

namespace urban_city_power_managment.Web.Services
{
    /// <summary>
    /// Gas monitoring service that uses Entity Framework Core with SQL Server
    /// </summary>
    public class GasMonitoringService : IGasMonitoringService
    {
        private readonly EnergyDbContext _dbContext;
        private readonly ILogger<GasMonitoringService> _logger;

     // Eindhoven gas zones
     private static readonly string[] GasZones = new[]
    {
       "Strijp", "Woensel", "Gestel", "Stratum", "Tongelre", "Centrum", "Meerhoven"
      };

        public GasMonitoringService(EnergyDbContext dbContext, ILogger<GasMonitoringService> logger)
   {
     _dbContext = dbContext;
    _logger = logger;
        }

      public async Task<GasUsage?> GetCurrentUsageAsync(string location)
 {
  try
 {
          return await _dbContext.GasUsage
    .Where(g => g.Location == location)
     .OrderByDescending(g => g.Timestamp)
         .FirstOrDefaultAsync();
 }
 catch (Exception ex)
         {
      _logger.LogWarning(ex, "Failed to fetch gas usage for {Location}", location);
               return null;
    }
        }

 public async Task<List<GasUsage>> GetAllCurrentUsageAsync()
   {
          try
       {
          // Get latest reading per zone
     var latestUsage = await _dbContext.GasUsage
    .GroupBy(g => g.Location)
            .Select(g => g.OrderByDescending(x => x.Timestamp).First())
   .ToListAsync();

  if (latestUsage.Any())
       return latestUsage;

          return GenerateMockGasUsage();
    }
    catch (Exception ex)
 {
         _logger.LogWarning(ex, "Failed to fetch gas usage, using mock data");
      return GenerateMockGasUsage();
    }
        }

        public async Task<List<GasLeakAlert>> GetActiveAlertsAsync()
  {
            try
         {
    var alerts = await _dbContext.GasLeakAlerts
         .Where(a => !a.IsResolved)
     .OrderByDescending(a => a.Severity)
 .ThenByDescending(a => a.DetectedAt)
     .ToListAsync();

     if (alerts.Any())
       return alerts;

         return GenerateMockAlerts().Where(a => !a.IsResolved).ToList();
      }
catch (Exception ex)
 {
  _logger.LogWarning(ex, "Failed to fetch active alerts, using mock data");
              return GenerateMockAlerts().Where(a => !a.IsResolved).ToList();
    }
        }

  public async Task<List<GasLeakAlert>> GetAllAlertsAsync()
        {
            try
    {
     var alerts = await _dbContext.GasLeakAlerts
     .OrderByDescending(a => a.DetectedAt)
       .ToListAsync();

         if (alerts.Any())
    return alerts;

      return GenerateMockAlerts();
    }
    catch (Exception ex)
   {
             _logger.LogWarning(ex, "Failed to fetch all alerts, using mock data");
    return GenerateMockAlerts();
     }
        }

 public async Task<bool> ResolveAlertAsync(string alertId)
   {
 try
  {
   var alert = await _dbContext.GasLeakAlerts.FindAsync(alertId);
    if (alert != null)
      {
 alert.IsResolved = true;
 alert.ResolvedAt = DateTime.UtcNow;
    await _dbContext.SaveChangesAsync();
       return true;
             }
 return false;
   }
catch (Exception ex)
   {
 _logger.LogError(ex, "Failed to resolve alert {AlertId}", alertId);
          return false;
   }
        }

  public async Task<double> GetTotalConsumptionAsync()
   {
     try
    {
         var latestUsage = await _dbContext.GasUsage
   .GroupBy(g => g.Location)
        .Select(g => g.OrderByDescending(x => x.Timestamp).First())
          .ToListAsync();

   if (latestUsage.Any())
   return (double)latestUsage.Sum(u => u.TotalConsumption);

 return 15234.5;
      }
            catch (Exception ex)
       {
         _logger.LogWarning(ex, "Failed to fetch total consumption, using mock data");
           return 15234.5;
    }
}

  #region Mock Data Generation

   private List<GasUsage> GenerateMockGasUsage()
        {
      var random = new Random();
            return GasZones.Select(zone => new GasUsage
 {
           Location = zone,
  FlowRate = (decimal)Math.Round(random.NextDouble() * 100 + 50, 2),
      Pressure = (decimal)Math.Round(random.NextDouble() * 0.5 + 1.8, 3),
      Temperature = (decimal)Math.Round(random.NextDouble() * 10 + 15, 1),
     TotalConsumption = (decimal)Math.Round(random.NextDouble() * 5000 + 2000, 2),
     Timestamp = DateTime.UtcNow,
                IsNormal = random.Next(0, 10) > 1
          }).ToList();
        }

     private List<GasLeakAlert> GenerateMockAlerts()
      {
   var random = new Random();
  var alerts = new List<GasLeakAlert>();
    var severities = new[] { AlertSeverity.Low, AlertSeverity.Medium, AlertSeverity.High, AlertSeverity.Critical };

    for (int i = 0; i < 5; i++)
            {
var severity = severities[random.Next(severities.Length)];
         alerts.Add(new GasLeakAlert
   {
     Id = Guid.NewGuid().ToString(),
       Location = GasZones[random.Next(GasZones.Length)],
   PressureDrop = (decimal)Math.Round(random.NextDouble() * 2 + 0.5, 2),
 FlowRateAnomaly = (decimal)Math.Round(random.NextDouble() * 50 + 10, 2),
         Severity = severity,
     DetectedAt = DateTime.UtcNow.AddHours(-random.Next(1, 48)),
IsResolved = random.Next(0, 10) > 6,
    Description = GetAlertDescription(severity),
       AffectedCustomers = random.Next(10, 500)
 });
            }

   return alerts;
        }

 private static string GetAlertDescription(AlertSeverity severity)
   {
     return severity switch
         {
    AlertSeverity.Low => "Kleine drukafwijking gedetecteerd, monitoring actief",
    AlertSeverity.Medium => "Matige drukdaling, inspectie aanbevolen",
    AlertSeverity.High => "Significante drukdaling, onmiddellijke inspectie vereist",
 AlertSeverity.Critical => "Kritiek gaslek gedetecteerd, noodprocedure geactiveerd",
       _ => "Gaslek gedetecteerd"
     };
        }

        #endregion
    }
}
