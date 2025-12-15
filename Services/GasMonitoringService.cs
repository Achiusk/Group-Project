using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using urban_city_power_managment.Models;

namespace urban_city_power_managment.Services
{
    /// <summary>
    /// Implementation of gas monitoring service with mock data
    /// Production version would integrate with real sensors and SCADA systems
    /// </summary>
    public class GasMonitoringService : IGasMonitoringService
    {
        private readonly List<GasUsage> _currentUsage;
        private readonly List<GasLeakAlert> _alerts;
        private readonly Random _random;

        public event EventHandler<GasLeakAlert>? LeakDetected;
        public event EventHandler<GasUsage>? UsageChanged;

        public GasMonitoringService()
        {
     _random = new Random();
  _currentUsage = new List<GasUsage>();
        _alerts = new List<GasLeakAlert>();
 
         InitializeMockData();
        }

      private void InitializeMockData()
        {
  // Initialize gas usage for different zones in Eindhoven
    var zones = new[]
            {
  "Strijp",
  "Woensel",
  "Tongelre",
         "Stratum",
     "Gestel",
     "Industrial Zone",
       "City Center"
      };

        foreach (var zone in zones)
          {
          _currentUsage.Add(new GasUsage
     {
         Location = zone,
      FlowRate = 1000 + _random.Next(0, 500), // m³/h
          Pressure = 4.0 + _random.NextDouble() * 0.5, // 4.0-4.5 bar (normal)
         Temperature = 10 + _random.Next(0, 15), // 10-25°C
  TotalConsumption = 50000 + _random.Next(0, 20000), // m³
       Timestamp = DateTime.Now,
          IsNormal = true
 });
   }

  // Add one mock gas leak alert for demonstration
    _alerts.Add(new GasLeakAlert
        {
   Location = "Tongelre - Kruisstraat",
     PressureDrop = 0.8, // 0.8 bar drop
    FlowRateAnomaly = 150, // 150 m³/h unexplained flow
        Severity = AlertSeverity.High,
  DetectedAt = DateTime.Now.AddHours(-2),
         IsResolved = false,
     Description = "Significant pressure drop detected. Possible underground leak.",
            AffectedCustomers = 45
            });
     }

        public async Task<GasUsage> GetCurrentUsageAsync(string location)
        {
    await Task.Delay(50); // Simulate async operation
    
            var usage = _currentUsage.FirstOrDefault(u => u.Location == location);
     if (usage == null)
            {
         throw new ArgumentException($"Location '{location}' not found");
 }

    // Simulate real-time data variation
     usage.FlowRate += _random.Next(-50, 50);
  usage.Pressure += (_random.NextDouble() - 0.5) * 0.1;
      usage.Timestamp = DateTime.Now;

            return usage;
        }

      public async Task<List<GasUsage>> GetAllCurrentUsageAsync()
  {
            await Task.Delay(100); // Simulate async operation

  // Update all readings with slight variations
 foreach (var usage in _currentUsage)
            {
     usage.FlowRate += _random.Next(-30, 30);
      usage.Pressure += (_random.NextDouble() - 0.5) * 0.05;
       usage.Temperature += _random.Next(-1, 2);
         usage.Timestamp = DateTime.Now;
     
             // Check if readings are abnormal
           usage.IsNormal = usage.Pressure >= 3.5 && usage.Pressure <= 5.0;
            }

      return new List<GasUsage>(_currentUsage);
        }

        public async Task<List<GasLeakAlert>> GetActiveAlertsAsync()
        {
    await Task.Delay(50);
     return _alerts.Where(a => !a.IsResolved).ToList();
        }

        public async Task<List<GasLeakAlert>> GetAllAlertsAsync()
        {
            await Task.Delay(50);
            return new List<GasLeakAlert>(_alerts);
        }

        public async Task<bool> CheckForLeaksAsync()
        {
await Task.Delay(200); // Simulate comprehensive system check

            bool leakDetected = false;

          foreach (var usage in _currentUsage)
          {
       // Check for abnormal pressure
     if (usage.Pressure < 3.5) // Pressure too low
 {
              var existingAlert = _alerts.FirstOrDefault(a => 
        a.Location.Contains(usage.Location) && !a.IsResolved);

           if (existingAlert == null)
          {
           var alert = new GasLeakAlert
       {
         Location = usage.Location,
   PressureDrop = 4.0 - usage.Pressure,
      FlowRateAnomaly = Math.Max(0, usage.FlowRate - 1000),
  Severity = DetermineSeverity(4.0 - usage.Pressure),
    DetectedAt = DateTime.Now,
           IsResolved = false,
               Description = $"Abnormal pressure detected in {usage.Location}. Investigating potential leak.",
         AffectedCustomers = _random.Next(10, 100)
       };

          _alerts.Add(alert);
        LeakDetected?.Invoke(this, alert);
  leakDetected = true;
        }
   }
  }

  return leakDetected;
        }

        public async Task<bool> ResolveAlertAsync(string alertId)
   {
            await Task.Delay(50);

    var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
            if (alert == null)
          {
       return false;
  }

  alert.IsResolved = true;
          alert.ResolvedAt = DateTime.Now;

 return true;
        }

        public async Task<double> GetTotalConsumptionAsync()
        {
            await Task.Delay(100);
            return _currentUsage.Sum(u => u.TotalConsumption);
        }

        private AlertSeverity DetermineSeverity(double pressureDrop)
      {
            if (pressureDrop < 0.2) return AlertSeverity.Low;
      if (pressureDrop < 0.5) return AlertSeverity.Medium;
    if (pressureDrop < 1.0) return AlertSeverity.High;
            return AlertSeverity.Critical;
        }
    }
}
