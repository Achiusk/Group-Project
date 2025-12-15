using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using urban_city_power_managment.Models;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class GasInfrastructureViewModel : ViewModelBase
    {
      private readonly ILocalizationService _localizationService;
        private readonly IGasMonitoringService _gasMonitoringService;
        
        private double _totalGasConsumption;
        private int _activeAlertsCount;
     private string _overallStatus;

        public GasInfrastructureViewModel(
     ILocalizationService localizationService,
  IGasMonitoringService gasMonitoringService)
{
      _localizationService = localizationService;
  _gasMonitoringService = gasMonitoringService;
     _overallStatus = "Normal";

     GasUsageData = new ObservableCollection<GasUsage>();
ActiveAlerts = new ObservableCollection<GasLeakAlert>();

        // Commands - Use MainThreadScheduler to ensure all notifications happen on UI thread
      // CRITICAL FIX: Use CreateFromTask WITHOUT outputScheduler to avoid complex observable chains
        RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
 
   CheckForLeaksCommand = ReactiveCommand.CreateFromTask(CheckForLeaksAsyncInternal);
 
     ResolveAlertCommand = ReactiveCommand.CreateFromTask<string>(ResolveAlertAsyncInternal);

      // Subscribe to leak detection events
      _gasMonitoringService.LeakDetected += OnLeakDetected;
      
      // CRITICAL: Do NOT start any async operations here
      // Data will be loaded on-demand when Refresh button is clicked
   }

        public ObservableCollection<GasUsage> GasUsageData { get; }
        public ObservableCollection<GasLeakAlert> ActiveAlerts { get; }

        public string Title => _localizationService.GetString("GasInfrastructure");
      public string GasUsageLabel => _localizationService.GetString("GasUsage");
        public string GasLeaksLabel => _localizationService.GetString("GasLeaks");
 public string PressureLabel => _localizationService.GetString("Pressure");
        public string FlowRateLabel => _localizationService.GetString("FlowRate");
 public string TemperatureLabel => _localizationService.GetString("GasTemperature");
        public string TotalConsumptionLabel => _localizationService.GetString("TotalGasConsumption");
        public string ActiveAlertsLabel => _localizationService.GetString("ActiveAlerts");
        public string LeakDetectedLabel => _localizationService.GetString("LeakDetected");
   public string PressureDropLabel => _localizationService.GetString("PressureDrop");
    public string SeverityLabel => _localizationService.GetString("Severity");
        public string ResolveAlertLabel => _localizationService.GetString("ResolveAlert");
        public string GasMonitoringLabel => _localizationService.GetString("GasMonitoring");
        public string ZoneStatusLabel => _localizationService.GetString("ZoneStatus");

        public double TotalGasConsumption
        {
            get => _totalGasConsumption;
            set => this.RaiseAndSetIfChanged(ref _totalGasConsumption, value);
        }

        public int ActiveAlertsCount
     {
       get => _activeAlertsCount;
  set => this.RaiseAndSetIfChanged(ref _activeAlertsCount, value);
 }

        public string OverallStatus
    {
            get => _overallStatus;
            set => this.RaiseAndSetIfChanged(ref _overallStatus, value);
   }

        public ReactiveCommand<Unit, Unit> RefreshDataCommand { get; }
        public ReactiveCommand<Unit, Unit> CheckForLeaksCommand { get; }
   public ReactiveCommand<string, Unit> ResolveAlertCommand { get; }

  // Public async methods for code-behind event handlers
        public async Task RefreshDataAsync()
   {
  await LoadDataAsync();
  }

 public async Task CheckForLeaksAsync()
   {
 await CheckForLeaksAsyncInternal();
        }

   public async Task ResolveAlertAsync(string alertId)
  {
 await ResolveAlertAsyncInternal(alertId);
 }

  private async Task LoadDataAsync()
        {
     try
    {
              // Load gas usage data for all zones
                var usageData = await _gasMonitoringService.GetAllCurrentUsageAsync();
     
  await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
         {
          GasUsageData.Clear();
      foreach (var usage in usageData)
   {
    GasUsageData.Add(usage);
          }
         });

          // Load active alerts
       var alerts = await _gasMonitoringService.GetActiveAlertsAsync();
             
     await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
          ActiveAlerts.Clear();
  foreach (var alert in alerts)
        {
             ActiveAlerts.Add(alert);
      }
       ActiveAlertsCount = alerts.Count;
 });

     // Get total consumption - Update on UI thread
 var totalConsumption = await _gasMonitoringService.GetTotalConsumptionAsync();
   await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
{
     TotalGasConsumption = totalConsumption;
 
     // Determine overall status
       UpdateOverallStatus();
});
       }
          catch (Exception ex)
        {
       // Log error (in production, use proper logging)
    Console.WriteLine($"Error loading gas data: {ex.Message}");
 }
        }

  private async Task CheckForLeaksAsyncInternal()
        {
   try
     {
        bool leakDetected = await _gasMonitoringService.CheckForLeaksAsync();
     
     // Reload alerts if leak was detected
       if (leakDetected)
      {
  await LoadDataAsync();
        }
    }
 catch (Exception ex)
   {
  Console.WriteLine($"Error checking for leaks: {ex.Message}");
      }
  }

        private async Task ResolveAlertAsyncInternal(string alertId)
        {
 try
      {
     bool resolved = await _gasMonitoringService.ResolveAlertAsync(alertId);
            
         if (resolved)
     {
// Remove from active alerts and update status on UI thread
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
    {
          var alert = ActiveAlerts.FirstOrDefault(a => a.Id == alertId);
  if (alert != null)
     {
       ActiveAlerts.Remove(alert);
     ActiveAlertsCount = ActiveAlerts.Count;
   }
         
      UpdateOverallStatus();
    });
       }
      }
  catch (Exception ex)
        {
    Console.WriteLine($"Error resolving alert: {ex.Message}");
       }
   }

        private void OnLeakDetected(object? sender, GasLeakAlert alert)
     {
   // Add new alert to the collection on UI thread
     Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
    ActiveAlerts.Insert(0, alert);
                ActiveAlertsCount = ActiveAlerts.Count;
      UpdateOverallStatus();
   });
        }

      private void UpdateOverallStatus()
      {
            if (ActiveAlertsCount == 0)
            {
        OverallStatus = "Normal";
        }
       else if (ActiveAlerts.Any(a => a.Severity == AlertSeverity.Critical))
     {
 OverallStatus = "Critical";
            }
        else if (ActiveAlerts.Any(a => a.Severity == AlertSeverity.High))
     {
       OverallStatus = "High Alert";
    }
       else
     {
    OverallStatus = "Warning";
      }
        }

        public string GetSeverityText(AlertSeverity severity)
        {
    return severity switch
            {
 AlertSeverity.Low => _localizationService.GetString("SeverityLow"),
                AlertSeverity.Medium => _localizationService.GetString("SeverityMedium"),
    AlertSeverity.High => _localizationService.GetString("SeverityHigh"),
 AlertSeverity.Critical => _localizationService.GetString("SeverityCritical"),
            _ => severity.ToString()
 };
        }
    }
}
