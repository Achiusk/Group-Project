using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using urban_city_power_managment.ViewModels;

namespace urban_city_power_managment.Views
{
    public partial class GasInfrastructureView : UserControl
    {
        private GasInfrastructureViewModel? ViewModel => DataContext as GasInfrastructureViewModel;

        public GasInfrastructureView()
        {
            InitializeComponent();
        }

        // Gas infrastructure event handlers - directly call ViewModel methods
        private async void OnRefreshDataClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                await ViewModel.RefreshDataAsync();
        }

        private async void OnCheckForLeaksClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                await ViewModel.CheckForLeaksAsync();
        }

        private async void OnResolveAlertClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string alertId && ViewModel != null)
                await ViewModel.ResolveAlertAsync(alertId);
        }
    }
}
