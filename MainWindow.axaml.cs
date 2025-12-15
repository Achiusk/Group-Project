using Avalonia.Controls;
using Avalonia.Interactivity;
using urban_city_power_managment.Services;
using urban_city_power_managment.ViewModels;

namespace urban_city_power_managment
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        // Navigation event handlers - NO ReactiveUI command bindings!
        private void OnDashboardClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new DashboardViewModel(ViewModel.GetLocalizationService());
        }

        private void OnPowerGenerationClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new PowerGenerationViewModel(ViewModel.GetLocalizationService());
        }

        private void OnConsumersClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new ConsumersViewModel(ViewModel.GetLocalizationService());
        }

        private void OnBusinessesClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new BusinessesViewModel(ViewModel.GetLocalizationService());
        }

        private void OnGasInfrastructureClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new GasInfrastructureViewModel(
                    ViewModel.GetLocalizationService(),
                    ViewModel.GetGasMonitoringService());
        }

        private void OnNotificationsClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new NotificationsViewModel(
                    ViewModel.GetLocalizationService(),
                    ViewModel.GetGasMonitoringService());
        }

        private void OnTasksClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new TasksViewModel(ViewModel.GetLocalizationService());
        }

        private void OnSettingsClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.CurrentView = new SettingsViewModel(
                    ViewModel.GetLocalizationService(),
                    ViewModel.GetThemeService());
        }
    }
}