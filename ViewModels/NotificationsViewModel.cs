using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
   private readonly ILocalizationService _localizationService;
        private readonly IGasMonitoringService? _gasMonitoringService;

        public NotificationsViewModel(ILocalizationService localizationService)
          : this(localizationService, null)
     {
     }

        public NotificationsViewModel(
  ILocalizationService localizationService,
        IGasMonitoringService? gasMonitoringService)
        {
   _localizationService = localizationService;
          _gasMonitoringService = gasMonitoringService;
          
  Notifications = new ObservableCollection<NotificationItem>
         {
  new NotificationItem 
 { 
          Type = "Incident",
       Message = "Power outage in district Noord - Investigation ongoing",
     Time = "10:30"
     },
          new NotificationItem 
    { 
    Type = "UnusualUsage",
     Message = "High consumption detected in industrial zone",
         Time = "09:15"
       }
          };

 // Subscribe to gas leak events if service is available
            if (_gasMonitoringService != null)
    {
   _gasMonitoringService.LeakDetected += OnGasLeakDetected;
      
      // CRITICAL: Do NOT start any async operations here
   // Notifications load on-demand
    }
        }

        public string Title => _localizationService.GetString("Notifications");
        public ObservableCollection<NotificationItem> Notifications { get; }

        private async Task LoadGasLeakAlertsAsync()
        {
  if (_gasMonitoringService == null) return;

 try
     {
var alerts = await _gasMonitoringService.GetActiveAlertsAsync();
             
    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
      {
   foreach (var alert in alerts)
   {
       Notifications.Insert(0, new NotificationItem
 {
         Type = "GasLeak",
    Message = $"{alert.Location}: {alert.Description}",
     Time = alert.DetectedAt.ToString("HH:mm")
});
      }
        });
         }
 catch (Exception ex)
    {
      Console.WriteLine($"Error loading gas leak alerts: {ex.Message}");
  }
        }

private void OnGasLeakDetected(object? sender, Models.GasLeakAlert alert)
    {
   Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
{
Notifications.Insert(0, new NotificationItem
          {
Type = "GasLeak"
,
     Message = $"{alert.Location}: {alert.Description}",
       Time = alert.DetectedAt.ToString("HH:mm")
         });
 });
    }
    }

    public class NotificationItem
    {
        public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
  }
}
