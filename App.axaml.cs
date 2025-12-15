using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using urban_city_power_managment.Services;
using urban_city_power_managment.ViewModels;

namespace urban_city_power_managment
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Setup services
                var localizationService = new LocalizationService();
                var themeService = new ThemeService();
                var gasMonitoringService = new GasMonitoringService();
                var weatherService = new OpenMeteoWeatherService();

                // Create ViewModel with services
                var mainViewModel = new MainWindowViewModel(
                    localizationService,
                    themeService,
                    gasMonitoringService,
                    weatherService);

                desktop.MainWindow = new MainWindow(mainViewModel);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}