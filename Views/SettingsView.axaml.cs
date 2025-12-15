using Avalonia.Controls;
using Avalonia.Interactivity;
using urban_city_power_managment.ViewModels;

namespace urban_city_power_managment.Views
{
    public partial class SettingsView : UserControl
  {
 private SettingsViewModel? ViewModel => DataContext as SettingsViewModel;

      public SettingsView()
  {
    InitializeComponent();
  
         // Debug: Check if DataContext is set
 this.DataContextChanged += (s, e) =>
            {
     System.Diagnostics.Debug.WriteLine($"SettingsView DataContext changed to: {DataContext?.GetType().Name ?? "null"}");
            };
  }

  // Language event handlers
private void OnSetDutchClick(object? sender, RoutedEventArgs e)
        {
        System.Diagnostics.Debug.WriteLine($"Dutch button clicked! ViewModel is {(ViewModel == null ? "null" : "not null")}");
      if (ViewModel != null)
   {
                System.Diagnostics.Debug.WriteLine("Calling SetLanguage('nl')...");
    ViewModel.SetLanguage("nl");
     System.Diagnostics.Debug.WriteLine("SetLanguage('nl') completed");
    }
  }

  private void OnSetEnglishClick(object? sender, RoutedEventArgs e)
     {
    System.Diagnostics.Debug.WriteLine($"English button clicked! ViewModel is {(ViewModel == null ? "null" : "not null")}");
            if (ViewModel != null)
        {
       System.Diagnostics.Debug.WriteLine("Calling SetLanguage('en')...");
 ViewModel.SetLanguage("en");
     System.Diagnostics.Debug.WriteLine("SetLanguage('en') completed");
   }
 }

        // Theme event handlers
   private void OnSetLightThemeClick(object? sender, RoutedEventArgs e)
  {
   System.Diagnostics.Debug.WriteLine($"Light theme button clicked! ViewModel is {(ViewModel == null ? "null" : "not null")}");
         if (ViewModel != null)
       {
    System.Diagnostics.Debug.WriteLine("Calling SetLightTheme()...");
      ViewModel.SetLightTheme();
           System.Diagnostics.Debug.WriteLine("SetLightTheme() completed");
     }
        }

  private void OnSetDarkThemeClick(object? sender, RoutedEventArgs e)
    {
     System.Diagnostics.Debug.WriteLine($"Dark theme button clicked! ViewModel is {(ViewModel == null ? "null" : "not null")}");
    if (ViewModel != null)
         {
    System.Diagnostics.Debug.WriteLine("Calling SetDarkTheme()...");
ViewModel.SetDarkTheme();
        System.Diagnostics.Debug.WriteLine("SetDarkTheme() completed");
 }
  }
  }
}
