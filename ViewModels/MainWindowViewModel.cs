using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILocalizationService _localizationService;
        private readonly IThemeService _themeService;
        private readonly IGasMonitoringService _gasMonitoringService;
        private readonly IWeatherService _weatherService;
        private ViewModelBase _currentView;
        private string _currentTime;
        private string _currentDate;
        private string _weatherInfo;
        private string _location;

        public MainWindowViewModel(
         ILocalizationService localizationService,
      IThemeService themeService,
      IGasMonitoringService gasMonitoringService,
         IWeatherService weatherService)
        {
            _localizationService = localizationService;
       _themeService = themeService;
            _gasMonitoringService = gasMonitoringService;
            _weatherService = weatherService;
            _location = _localizationService.GetString("Eindhoven");
  _weatherInfo = "Loading...";
 _currentTime = DateTime.Now.ToString("HH:mm");
  _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

 // Initialize with Dashboard view
_currentView = new DashboardViewModel(_localizationService);

         // CRITICAL FIX: Create commands synchronously WITHOUT observable chains
       // Use simple Create instead of CreateFromTask to avoid async observable setup
            NavigateToDashboardCommand = ReactiveCommand.Create(NavigateToDashboard);
   NavigateToPowerGenerationCommand = ReactiveCommand.Create(NavigateToPowerGeneration);
            NavigateToConsumersCommand = ReactiveCommand.Create(NavigateToConsumers);
   NavigateToBusinessesCommand = ReactiveCommand.Create(NavigateToBusinesses);
            NavigateToGasInfrastructureCommand = ReactiveCommand.Create(NavigateToGasInfrastructure);
            NavigateToNotificationsCommand = ReactiveCommand.Create(NavigateToNotifications);
   NavigateToTasksCommand = ReactiveCommand.Create(NavigateToTasks);
     NavigateToSettingsCommand = ReactiveCommand.Create(NavigateToSettings);

 // Subscribe to language changes to update main window labels
            _localizationService.LanguageChanged += OnLanguageChanged;

            // Set initial theme
            System.Diagnostics.Debug.WriteLine("MainWindowViewModel: Setting initial theme to Light...");
  _themeService.SetTheme(Avalonia.Styling.ThemeVariant.Light);

    // Update time every minute
       StartTimeUpdater();
      
  // CRITICAL: Do NOT start any async operations here
  // Weather loads in background via StartTimeUpdater after first delay
 }

     private void OnLanguageChanged(object? sender, EventArgs e)
        {
    System.Diagnostics.Debug.WriteLine("=== MainWindowViewModel.OnLanguageChanged fired ===");
   // Refresh localized properties when language changes
      this.RaisePropertyChanged(nameof(AppTitle));
          this.RaisePropertyChanged(nameof(Location));
         this.RaisePropertyChanged(nameof(DashboardLabel));
        this.RaisePropertyChanged(nameof(PowerGenerationLabel));
   this.RaisePropertyChanged(nameof(ConsumersLabel));
     this.RaisePropertyChanged(nameof(BusinessesLabel));
     this.RaisePropertyChanged(nameof(GasInfrastructureLabel));
     this.RaisePropertyChanged(nameof(NotificationsLabel));
        this.RaisePropertyChanged(nameof(TasksLabel));
      this.RaisePropertyChanged(nameof(SettingsLabel));
      
    // Re-create the current view with new language
            var currentViewType = _currentView?.GetType();
            System.Diagnostics.Debug.WriteLine($"Current view type: {currentViewType?.Name ?? "null"}");
  
 if (currentViewType == typeof(DashboardViewModel))
            {
 System.Diagnostics.Debug.WriteLine("Recreating DashboardViewModel...");
           NavigateToDashboard();
         }
    else if (currentViewType == typeof(PowerGenerationViewModel))
          {
        System.Diagnostics.Debug.WriteLine("Recreating PowerGenerationViewModel...");
      NavigateToPowerGeneration();
        }
        else if (currentViewType == typeof(ConsumersViewModel))
  {
             System.Diagnostics.Debug.WriteLine("Recreating ConsumersViewModel...");
     NavigateToConsumers();
            }
          else if (currentViewType == typeof(BusinessesViewModel))
        {
          System.Diagnostics.Debug.WriteLine("Recreating BusinessesViewModel...");
             NavigateToBusinesses();
    }
    else if (currentViewType == typeof(GasInfrastructureViewModel))
            {
        System.Diagnostics.Debug.WriteLine("Recreating GasInfrastructureViewModel...");
  NavigateToGasInfrastructure();
   }
    else if (currentViewType == typeof(NotificationsViewModel))
         {
    System.Diagnostics.Debug.WriteLine("Recreating NotificationsViewModel...");
      NavigateToNotifications();
          }
       else if (currentViewType == typeof(TasksViewModel))
        {
      System.Diagnostics.Debug.WriteLine("Recreating TasksViewModel...");
        NavigateToTasks();
            }
else if (currentViewType == typeof(SettingsViewModel))
            {
             System.Diagnostics.Debug.WriteLine("Recreating SettingsViewModel...");
    NavigateToSettings();
     }
            
            System.Diagnostics.Debug.WriteLine("=== Language change handling complete ===");
        }

     public ViewModelBase CurrentView
      {
  get => _currentView;
    set => this.RaiseAndSetIfChanged(ref _currentView, value);
     }

        public string CurrentTime
        {
         get => _currentTime;
  set => this.RaiseAndSetIfChanged(ref _currentTime, value);
      }

        public string CurrentDate
  {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
      }

        public string WeatherInfo
        {
        get => _weatherInfo;
          set => this.RaiseAndSetIfChanged(ref _weatherInfo, value);
        }

        public string Location
        {
            get => _location;
    set => this.RaiseAndSetIfChanged(ref _location, value);
        }

        public string AppTitle => _localizationService.GetString("AppTitle");
        
        // Navigation menu labels
        public string DashboardLabel => _localizationService.GetString("Dashboard");
        public string PowerGenerationLabel => _localizationService.GetString("PowerGeneration");
        public string ConsumersLabel => _localizationService.GetString("Consumers");
        public string BusinessesLabel => _localizationService.GetString("Businesses");
  public string GasInfrastructureLabel => _localizationService.GetString("GasInfrastructure");
   public string NotificationsLabel => _localizationService.GetString("Notifications");
        public string TasksLabel => _localizationService.GetString("Tasks");
        public string SettingsLabel => _localizationService.GetString("Settings");

 // Helper methods to expose services for code-behind event handlers
        public ILocalizationService GetLocalizationService() => _localizationService;
   public IThemeService GetThemeService() => _themeService;
   public IGasMonitoringService GetGasMonitoringService() => _gasMonitoringService;

        // Navigation Commands
        public ReactiveCommand<Unit, Unit> NavigateToDashboardCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToPowerGenerationCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToConsumersCommand { get; }
  public ReactiveCommand<Unit, Unit> NavigateToBusinessesCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToGasInfrastructureCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToNotificationsCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToTasksCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }

     private void NavigateToDashboard() => 
 CurrentView = new DashboardViewModel(_localizationService);

     private void NavigateToPowerGeneration() => 
   CurrentView = new PowerGenerationViewModel(_localizationService);

        private void NavigateToConsumers() => 
        CurrentView = new ConsumersViewModel(_localizationService);

        private void NavigateToBusinesses() => 
          CurrentView = new BusinessesViewModel(_localizationService);

        private void NavigateToGasInfrastructure() => 
          CurrentView = new GasInfrastructureViewModel(_localizationService, _gasMonitoringService);

        private void NavigateToNotifications() => 
   CurrentView = new NotificationsViewModel(_localizationService, _gasMonitoringService);

        private void NavigateToTasks() => 
            CurrentView = new TasksViewModel(_localizationService);

        private void NavigateToSettings() => 
    CurrentView = new SettingsViewModel(_localizationService, _themeService);

  private async void StartTimeUpdater()
        {
         // Load weather on first run
        await LoadWeatherAsync();
        
    while (true)
            {
    await Task.Delay(60000); // Update every minute
        
          // CRITICAL FIX: Marshal property updates to the UI thread
          await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
     {
      CurrentTime = DateTime.Now.ToString("HH:mm");
      CurrentDate = DateTime.Now.ToString("dd MMMM yyyy");
  });
   }
    }

 private async Task LoadWeatherAsync()
    {
try
{
  var weather = await _weatherService.GetCurrentWeatherAsync();
       
 await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
   {
   WeatherInfo = $"{weather.Description}, {weather.Temperature:F1}°C";
   });
     }
 catch (Exception ex)
    {
    Console.WriteLine($"Error loading weather: {ex.Message}");
      await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
     {
       WeatherInfo = "Weather unavailable";
  });
   }
 }
    }
}
